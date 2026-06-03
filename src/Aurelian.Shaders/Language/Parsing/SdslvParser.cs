using Aurelian.Shaders.Language.Ast;
using Aurelian.Shaders.Language.Diagnostics;
using Aurelian.Shaders.Language.Lexing;
using Aurelian.Shaders.Language.Tokens;

namespace Aurelian.Shaders.Language.Parsing;

public static class SdslvParser
{
    public static SdslvParseResult ParseModule(string source)
    {
        var lexResult = SdslvLexer.Lex(source);
        var parser = new Parser(lexResult.Tokens, lexResult.Diagnostics);
        return parser.ParseModule();
    }

    private sealed class Parser(IReadOnlyList<SdslvToken> tokens, IReadOnlyList<SdslvDiagnostic> lexDiagnostics)
    {
        private readonly List<SdslvDiagnostic> _diagnostics = [..lexDiagnostics];
        private int _index;

        public SdslvParseResult ParseModule()
        {
            SdslvPath? ns = null;
            var uses = new List<SdslvUseDecl>();
            var declarations = new List<SdslvDecl>();

            if (Match(SdslvTokenKind.KeywordNamespace))
            {
                ns = ParsePath("Expected namespace path after 'namespace'.");
                Expect(SdslvTokenKind.Semicolon, "Expected ';' after namespace declaration.");
            }

            while (Match(SdslvTokenKind.KeywordUse))
            {
                var path = ParsePath("Expected use path after 'use'.");
                if (path is not null)
                {
                    uses.Add(new SdslvUseDecl(path));
                }

                Expect(SdslvTokenKind.Semicolon, "Expected ';' after use declaration.");
            }

            while (!AtEnd)
            {
                var before = _index;
                var declaration = ParseDeclaration();
                if (declaration is not null)
                {
                    declarations.Add(declaration);
                }

                if (_index == before)
                {
                    ErrorHere("Unexpected token at module scope.");
                    Advance();
                }
            }

            var module = new SdslvModule(ns, uses, declarations);
            return new SdslvParseResult(module, _diagnostics);
        }

        private SdslvDecl? ParseDeclaration()
        {
            if (Match(SdslvTokenKind.KeywordType)) return ParseTypeAlias();
            if (Match(SdslvTokenKind.KeywordRecord)) return ParseFieldAggregate(isStream: false);
            if (Match(SdslvTokenKind.KeywordStream)) return ParseFieldAggregate(isStream: true);
            if (Match(SdslvTokenKind.KeywordEnum)) return ParseEnum();
            if (Match(SdslvTokenKind.KeywordShader)) return ParseShader();

            if (Current.Kind is SdslvTokenKind.KeywordFlow or SdslvTokenKind.KeywordInterface or SdslvTokenKind.KeywordCompile)
            {
                ErrorHere($"'{Current.Text}' declarations are not supported by parser M0.");
                SkipDeclarationLikeConstruct();
                return null;
            }

            return null;
        }

        private SdslvTypeAliasDecl? ParseTypeAlias()
        {
            var name = ParseIdentifier("Expected type alias name.");
            if (name is null)
            {
                SynchronizeDeclaration();
                return null;
            }

            Expect(SdslvTokenKind.Equals, "Expected '=' in type alias declaration.");
            var target = ParseTypeRef("Expected type reference in type alias declaration.");
            Expect(SdslvTokenKind.Semicolon, "Expected ';' after type alias declaration.");
            return target is null ? null : new SdslvTypeAliasDecl(name, target);
        }

        private SdslvDecl? ParseFieldAggregate(bool isStream)
        {
            var name = ParseIdentifier(isStream ? "Expected stream name." : "Expected record name.");
            if (name is null)
            {
                SynchronizeDeclaration();
                return null;
            }

            var fields = ParseFieldBlock(isStream ? "stream" : "record");
            return isStream ? new SdslvStreamDecl(name, fields) : new SdslvRecordDecl(name, fields);
        }

        private IReadOnlyList<SdslvFieldDecl> ParseFieldBlock(string owner)
        {
            var fields = new List<SdslvFieldDecl>();
            if (!Expect(SdslvTokenKind.LeftBrace, $"Expected '{{' after {owner} name."))
            {
                SynchronizeDeclaration();
                return fields;
            }

            while (!AtEnd && !Check(SdslvTokenKind.RightBrace))
            {
                var fieldName = ParseIdentifier($"Expected field name in {owner} declaration.");
                if (fieldName is null)
                {
                    SynchronizeMember();
                    continue;
                }

                Expect(SdslvTokenKind.Colon, "Expected ':' after field name.");
                var type = ParseTypeRef("Expected field type.");
                if (type is not null)
                {
                    fields.Add(new SdslvFieldDecl(fieldName, type));
                }

                if (!Match(SdslvTokenKind.Semicolon) && !Match(SdslvTokenKind.Comma))
                {
                    ErrorHere("Expected ';' after field declaration.");
                    SynchronizeMember();
                }
            }

            Expect(SdslvTokenKind.RightBrace, $"Expected '}}' after {owner} declaration.");
            return fields;
        }

        private SdslvEnumDecl? ParseEnum()
        {
            var start = Previous.Span;
            var name = ParseIdentifier("Expected enum name.");
            if (name is null)
            {
                SynchronizeDeclaration();
                return null;
            }

            var variants = new List<SdslvEnumVariant>();
            if (!Expect(SdslvTokenKind.LeftBrace, "Expected '{' after enum name."))
            {
                SynchronizeDeclaration();
                return new SdslvEnumDecl(name, variants, start);
            }

            while (!AtEnd && !Check(SdslvTokenKind.RightBrace))
            {
                var variantStart = Current.Span;
                var variant = ParseIdentifier("Expected enum variant name.");
                if (variant is not null)
                {
                    variants.Add(new SdslvEnumVariant(variant, variantStart));
                }

                if (!Match(SdslvTokenKind.Semicolon) && !Match(SdslvTokenKind.Comma))
                {
                    ErrorHere("Expected ';' or ',' after enum variant.");
                    SynchronizeMember();
                }
            }

            Expect(SdslvTokenKind.RightBrace, "Expected '}' after enum declaration.");
            return new SdslvEnumDecl(name, variants, start);
        }

        private SdslvShaderDecl? ParseShader()
        {
            var name = ParseIdentifier("Expected shader name.");
            if (name is null)
            {
                SynchronizeDeclaration();
                return null;
            }

            var generics = ParseOptionalGenericParameters();
            var implements = new List<SdslvPath>();
            if (Match(SdslvTokenKind.KeywordImplements))
            {
                do
                {
                    var implemented = ParsePath("Expected implemented interface path.");
                    if (implemented is not null)
                    {
                        implements.Add(implemented);
                    }
                }
                while (Match(SdslvTokenKind.Comma));
            }

            var constraints = ParseOptionalWhereConstraints();
            var materialFields = new List<SdslvFieldDecl>();
            var methods = new List<SdslvFunctionDecl>();
            var stageMethods = new List<SdslvFunctionDecl>();

            if (!Expect(SdslvTokenKind.LeftBrace, "Expected '{' after shader header."))
            {
                SynchronizeDeclaration();
                return new SdslvShaderDecl(name, generics, implements, constraints, materialFields, methods, stageMethods);
            }

            while (!AtEnd && !Check(SdslvTokenKind.RightBrace))
            {
                if (Match(SdslvTokenKind.KeywordMaterial))
                {
                    ParseMaterial(materialFields);
                }
                else if (Match(SdslvTokenKind.KeywordStage))
                {
                    var stageMethod = ParseStageMethod();
                    if (stageMethod is not null)
                    {
                        stageMethods.Add(stageMethod);
                    }
                }
                else if (Match(SdslvTokenKind.KeywordOverride))
                {
                    var method = ParseFunction(stage: null, isOverride: true, requireKeyword: true);
                    if (method is not null)
                    {
                        methods.Add(method);
                    }
                }
                else if (Check(SdslvTokenKind.KeywordFn))
                {
                    var method = ParseFunction(stage: null, isOverride: false, requireKeyword: true);
                    if (method is not null)
                    {
                        methods.Add(method);
                    }
                }
                else
                {
                    ErrorHere("Unexpected token in shader body.");
                    SkipDeclarationLikeConstruct();
                }
            }

            Expect(SdslvTokenKind.RightBrace, "Expected '}' after shader declaration.");
            return new SdslvShaderDecl(name, generics, implements, constraints, materialFields, methods, stageMethods);
        }

        private IReadOnlyList<string> ParseOptionalGenericParameters()
        {
            var generics = new List<string>();
            if (!Match(SdslvTokenKind.LeftAngle))
            {
                return generics;
            }

            while (!AtEnd && !Check(SdslvTokenKind.RightAngle))
            {
                var parameter = ParseIdentifier("Expected generic parameter name.");
                if (parameter is not null)
                {
                    generics.Add(parameter);
                }

                if (!Match(SdslvTokenKind.Comma))
                {
                    break;
                }
            }

            Expect(SdslvTokenKind.RightAngle, "Expected '>' after generic parameter list.");
            return generics;
        }

        private IReadOnlyList<SdslvWhereConstraint> ParseOptionalWhereConstraints()
        {
            var constraints = new List<SdslvWhereConstraint>();
            if (!Match(SdslvTokenKind.KeywordWhere))
            {
                return constraints;
            }

            do
            {
                var name = ParseIdentifier("Expected where constraint parameter name.");
                Expect(SdslvTokenKind.Colon, "Expected ':' in where constraint.");
                var bounds = new List<SdslvPath>();
                do
                {
                    var bound = ParsePath("Expected where constraint bound.");
                    if (bound is not null)
                    {
                        bounds.Add(bound);
                    }
                }
                while (Match(SdslvTokenKind.Plus));

                if (name is not null)
                {
                    constraints.Add(new SdslvWhereConstraint(name, bounds));
                }
            }
            while (Match(SdslvTokenKind.Comma));

            return constraints;
        }

        private void ParseMaterial(List<SdslvFieldDecl> materialFields)
        {
            if (Match(SdslvTokenKind.LeftBrace))
            {
                while (!AtEnd && !Check(SdslvTokenKind.RightBrace))
                {
                    ParseMaterialField(materialFields);
                }

                Expect(SdslvTokenKind.RightBrace, "Expected '}' after material block.");
                return;
            }

            ParseMaterialField(materialFields);
        }

        private void ParseMaterialField(List<SdslvFieldDecl> materialFields)
        {
            var fieldName = ParseIdentifier("Expected material field name.");
            if (fieldName is null)
            {
                SynchronizeMember();
                return;
            }

            Expect(SdslvTokenKind.Colon, "Expected ':' after material field name.");
            var type = ParseTypeRef("Expected material field type.");
            if (type is not null)
            {
                materialFields.Add(new SdslvFieldDecl(fieldName, type));
            }

            Expect(SdslvTokenKind.Semicolon, "Expected ';' after material field.");
        }

        private SdslvFunctionDecl? ParseStageMethod()
        {
            var stageOrName = ParseIdentifier("Expected stage name or stage function name.");
            if (stageOrName is null)
            {
                SynchronizeMember();
                return null;
            }

            if (Check(SdslvTokenKind.KeywordFn))
            {
                return ParseFunction(stageOrName, isOverride: false, requireKeyword: true);
            }

            return ParseFunction(stageOrName, isOverride: false, requireKeyword: false, alreadyReadName: stageOrName);
        }

        private SdslvFunctionDecl? ParseFunction(string? stage, bool isOverride, bool requireKeyword, string? alreadyReadName = null)
        {
            if (requireKeyword && !Expect(SdslvTokenKind.KeywordFn, "Expected 'fn' before function name."))
            {
                SynchronizeMember();
                return null;
            }

            var name = alreadyReadName ?? ParseIdentifier("Expected function name.");
            if (name is null)
            {
                SynchronizeMember();
                return null;
            }

            Expect(SdslvTokenKind.LeftParen, "Expected '(' in function signature.");
            var parameters = ParseParameterList();
            Expect(SdslvTokenKind.RightParen, "Expected ')' after function parameters.");

            if (!Match(SdslvTokenKind.Arrow) && !Match(SdslvTokenKind.Colon))
            {
                ErrorHere("Expected return type marker '->' or ':' in function signature.");
            }

            var returnType = ParseTypeRef("Expected function return type.") ?? new SdslvNamedTypeRef(new SdslvPath("void"));
            var errorType = Match(SdslvTokenKind.Bang) ? ParseTypeRef("Expected error type after '!'.") : null;
            var body = ParseOptionalBody();
            return new SdslvFunctionDecl(isOverride, stage, name, parameters, returnType, errorType, body);
        }

        private IReadOnlyList<SdslvFunctionParameter> ParseParameterList()
        {
            var parameters = new List<SdslvFunctionParameter>();
            while (!AtEnd && !Check(SdslvTokenKind.RightParen))
            {
                var name = ParseIdentifier("Expected parameter name.");
                Expect(SdslvTokenKind.Colon, "Expected ':' after parameter name.");
                var type = ParseTypeRef("Expected parameter type.");
                if (name is not null && type is not null)
                {
                    parameters.Add(new SdslvFunctionParameter(name, type));
                }

                if (!Match(SdslvTokenKind.Comma))
                {
                    break;
                }
            }

            return parameters;
        }

        private SdslvBody? ParseOptionalBody()
        {
            if (Match(SdslvTokenKind.Semicolon))
            {
                return null;
            }

            if (!Match(SdslvTokenKind.LeftBrace))
            {
                ErrorHere("Expected ';' or function body.");
                SynchronizeMember();
                return null;
            }

            var start = Previous.Span;
            var statements = new List<SdslvStatement>();
            while (!AtEnd && !Check(SdslvTokenKind.RightBrace))
            {
                var before = _index;
                var statement = ParseStatement();
                if (statement is not null)
                {
                    statements.Add(statement);
                }

                if (_index == before)
                {
                    ErrorHere("Unexpected token in function body.");
                    Advance();
                }
            }

            Expect(SdslvTokenKind.RightBrace, "Expected '}' after function body.");
            return new SdslvBody(statements, start with { End = Previous.Span.End });
        }

        private SdslvStatement? ParseStatement()
        {
            if (Match(SdslvTokenKind.Semicolon)) return new SdslvEmptyStatement();
            if (Match(SdslvTokenKind.KeywordReturn))
            {
                var value = ParseExpression() ?? new SdslvIdentifierExpression("<error>");
                Expect(SdslvTokenKind.Semicolon, "Expected ';' after return statement.");
                return new SdslvReturnStatement(value);
            }

            if (Match(SdslvTokenKind.KeywordLet))
            {
                var name = ParseIdentifier("Expected local name after 'let'.") ?? "<error>";
                Expect(SdslvTokenKind.Colon, "Expected ':' after local name.");
                var type = ParseTypeRef("Expected local type.") ?? new SdslvNamedTypeRef(new SdslvPath("<error>"));
                SdslvExpression? initializer = null;
                if (Match(SdslvTokenKind.Equals))
                {
                    initializer = ParseExpression();
                }

                Expect(SdslvTokenKind.Semicolon, "Expected ';' after let statement.");
                return new SdslvLetStatement(name, type, initializer);
            }

            var expression = ParseExpression();
            if (expression is null)
            {
                SynchronizeMember();
                return null;
            }

            if (Match(SdslvTokenKind.Equals))
            {
                var value = ParseExpression() ?? new SdslvIdentifierExpression("<error>");
                Expect(SdslvTokenKind.Semicolon, "Expected ';' after assignment.");
                return new SdslvAssignStatement(expression, value);
            }

            Expect(SdslvTokenKind.Semicolon, "Expected ';' after expression statement.");
            return new SdslvExpressionStatement(expression);
        }

        private SdslvExpression? ParseExpression(int minimumPrecedence = 0)
        {
            var left = ParseUnaryOrPrimary();
            if (left is null)
            {
                return null;
            }

            while (TryGetBinaryOperator(Current.Kind, out var op, out var precedence) && precedence >= minimumPrecedence)
            {
                Advance();
                var right = ParseExpression(precedence + 1);
                if (right is null)
                {
                    ErrorHere("Expected expression after binary operator.");
                    return left;
                }

                left = new SdslvBinaryExpression(left, op, right);
            }

            return left;
        }

        private SdslvExpression? ParseUnaryOrPrimary()
        {
            if (Match(SdslvTokenKind.Minus))
            {
                var operand = ParseUnaryOrPrimary() ?? new SdslvIdentifierExpression("<error>");
                return new SdslvUnaryExpression(SdslvUnaryOperator.Negate, operand);
            }

            var expression = ParsePrimary();
            while (expression is not null)
            {
                if (Match(SdslvTokenKind.Dot))
                {
                    var field = ParseIdentifier("Expected field name after '.'.") ?? "<error>";
                    expression = new SdslvFieldAccessExpression(expression, field);
                }
                else if (Match(SdslvTokenKind.LeftParen))
                {
                    var args = new List<SdslvExpression>();
                    while (!AtEnd && !Check(SdslvTokenKind.RightParen))
                    {
                        var arg = ParseExpression();
                        if (arg is not null)
                        {
                            args.Add(arg);
                        }

                        if (!Match(SdslvTokenKind.Comma))
                        {
                            break;
                        }
                    }

                    Expect(SdslvTokenKind.RightParen, "Expected ')' after call arguments.");
                    expression = new SdslvCallExpression(expression, args);
                }
                else if (Match(SdslvTokenKind.LeftBracket))
                {
                    var index = ParseExpression() ?? new SdslvIdentifierExpression("<error>");
                    Expect(SdslvTokenKind.RightBracket, "Expected ']' after index expression.");
                    expression = new SdslvIndexExpression(expression, index);
                }
                else
                {
                    break;
                }
            }

            return expression;
        }

        private SdslvExpression? ParsePrimary()
        {
            if (Match(SdslvTokenKind.Identifier)) return new SdslvIdentifierExpression(Previous.Text);
            if (Match(SdslvTokenKind.IntegerLiteral)) return new SdslvIntegerLiteralExpression(Previous.Text);
            if (Match(SdslvTokenKind.FloatLiteral)) return new SdslvFloatLiteralExpression(Previous.Text);
            if (Match(SdslvTokenKind.StringLiteral)) return new SdslvStringLiteralExpression(Previous.Text);
            if (Match(SdslvTokenKind.BoolLiteral)) return new SdslvBoolLiteralExpression(bool.Parse(Previous.Text));
            if (Match(SdslvTokenKind.KeywordTrue)) return new SdslvBoolLiteralExpression(true);
            if (Match(SdslvTokenKind.KeywordFalse)) return new SdslvBoolLiteralExpression(false);
            if (Match(SdslvTokenKind.LeftParen))
            {
                var expression = ParseExpression();
                Expect(SdslvTokenKind.RightParen, "Expected ')' after expression.");
                return expression;
            }

            ErrorHere("Expected expression.");
            return null;
        }

        private static bool TryGetBinaryOperator(SdslvTokenKind kind, out SdslvBinaryOperator op, out int precedence)
        {
            (op, precedence) = kind switch
            {
                SdslvTokenKind.Star => (SdslvBinaryOperator.Multiply, 40),
                SdslvTokenKind.Slash => (SdslvBinaryOperator.Divide, 40),
                SdslvTokenKind.Plus => (SdslvBinaryOperator.Add, 30),
                SdslvTokenKind.Minus => (SdslvBinaryOperator.Subtract, 30),
                SdslvTokenKind.EqEq => (SdslvBinaryOperator.Equal, 20),
                SdslvTokenKind.BangEq => (SdslvBinaryOperator.NotEqual, 20),
                SdslvTokenKind.LeftAngle => (SdslvBinaryOperator.Less, 20),
                SdslvTokenKind.LessEq => (SdslvBinaryOperator.LessEqual, 20),
                SdslvTokenKind.RightAngle => (SdslvBinaryOperator.Greater, 20),
                SdslvTokenKind.GreaterEq => (SdslvBinaryOperator.GreaterEqual, 20),
                _ => (default, -1),
            };
            return precedence >= 0;
        }

        private SdslvTypeRef? ParseTypeRef(string message)
        {
            if (Match(SdslvTokenKind.KeywordArray))
            {
                var start = Previous.Span;
                Expect(SdslvTokenKind.LeftAngle, "Expected '<' after array.");
                var element = ParseTypeRef("Expected array element type.");
                Expect(SdslvTokenKind.Comma, "Expected ',' after array element type.");
                var length = 0;
                if (Match(SdslvTokenKind.IntegerLiteral))
                {
                    _ = int.TryParse(Previous.Text, out length);
                }
                else
                {
                    ErrorHere("Expected array length integer literal.");
                }

                Expect(SdslvTokenKind.RightAngle, "Expected '>' after array type.");
                return element is null ? null : new SdslvArrayTypeRef(element, length, start with { End = Previous.Span.End });
            }

            var path = ParsePath(message);
            return path is null ? null : new SdslvNamedTypeRef(path);
        }

        private SdslvPath? ParsePath(string message)
        {
            var segments = new List<string>();
            var first = ParseIdentifier(message);
            if (first is null)
            {
                return null;
            }

            segments.Add(first);
            while (Match(SdslvTokenKind.Dot))
            {
                var next = ParseIdentifier("Expected path segment after '.'.");
                if (next is null)
                {
                    break;
                }

                segments.Add(next);
            }

            return new SdslvPath(segments);
        }

        private string? ParseIdentifier(string message)
        {
            if (Match(SdslvTokenKind.Identifier))
            {
                return Previous.Text;
            }

            ErrorHere(message);
            return null;
        }

        private void SkipDeclarationLikeConstruct()
        {
            if (Match(SdslvTokenKind.Semicolon))
            {
                return;
            }

            if (Match(SdslvTokenKind.LeftBrace))
            {
                var depth = 1;
                while (!AtEnd && depth > 0)
                {
                    if (Match(SdslvTokenKind.LeftBrace)) depth++;
                    else if (Match(SdslvTokenKind.RightBrace)) depth--;
                    else Advance();
                }

                return;
            }

            while (!AtEnd && !Check(SdslvTokenKind.Semicolon) && !Check(SdslvTokenKind.RightBrace))
            {
                Advance();
            }

            _ = Match(SdslvTokenKind.Semicolon);
        }

        private void SynchronizeDeclaration()
        {
            while (!AtEnd && !Check(SdslvTokenKind.Semicolon) && !Check(SdslvTokenKind.RightBrace))
            {
                Advance();
            }

            _ = Match(SdslvTokenKind.Semicolon);
        }

        private void SynchronizeMember()
        {
            while (!AtEnd && !Check(SdslvTokenKind.Semicolon) && !Check(SdslvTokenKind.Comma) && !Check(SdslvTokenKind.RightBrace))
            {
                Advance();
            }

            _ = Match(SdslvTokenKind.Semicolon) || Match(SdslvTokenKind.Comma);
        }

        private bool Expect(SdslvTokenKind kind, string message)
        {
            if (Match(kind))
            {
                return true;
            }

            ErrorHere(message);
            return false;
        }

        private bool Match(SdslvTokenKind kind)
        {
            if (!Check(kind))
            {
                return false;
            }

            Advance();
            return true;
        }

        private bool Check(SdslvTokenKind kind) => Current.Kind == kind;
        private bool AtEnd => Current.Kind == SdslvTokenKind.EndOfFile;
        private SdslvToken Current => tokens[Math.Min(_index, tokens.Count - 1)];
        private SdslvToken Previous => tokens[Math.Max(0, _index - 1)];

        private SdslvToken Advance()
        {
            if (!AtEnd)
            {
                _index++;
            }

            return Previous;
        }

        private void ErrorHere(string message) => _diagnostics.Add(new SdslvDiagnostic(
            "SDSL-P001",
            SdslvDiagnosticSeverity.Error,
            SdslvDiagnosticPhase.Parsing,
            message,
            Current.Span));
    }
}
