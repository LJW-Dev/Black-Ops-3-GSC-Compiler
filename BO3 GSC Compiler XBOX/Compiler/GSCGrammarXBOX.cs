using Irony.Parsing;

namespace GSCCompilerXBOX
{
    /// <summary>
    ///     The grammar of the GameScript language.
    /// </summary>
    [Language("GameScript", "1.0", "GameScript grammar for Call of Duty: Black Ops 3 PC")]
    public class GSCGrammar : Grammar
    {
        public GSCGrammar() : base(false) //change case sensitivity to false
        {
            #region Lexical structure

            //Comments
            var blockComment = new CommentTerminal("block-comment", "/*", "*/");
            var lineComment = new CommentTerminal("line-comment", "//",
                "\r", "\n", "\u2085", "\u2028", "\u2029");
            NonGrammarTerminals.Add(blockComment);
            NonGrammarTerminals.Add(lineComment);

            //Literals
            var numberLiteral = new NumberLiteral("numberLiteral", NumberOptions.AllowSign);
            var stringLiteral = new StringLiteral("stringLiteral", "\"");
            var identifier = new IdentifierTerminal("identifier", @"_0123456789", "_");
            var pathIdentifier = new IdentifierTerminal("identifier", @"._/\", "_");

            MarkPunctuation("(", ")", "{", "}", "[", "]", ",", ".", ";", "::", "[[", "]]", "#include", "#define", "#namespace", "#checksum", "#path", "#using_animtree");

            RegisterOperators(1, "*", "/", "%");
            RegisterOperators(2, "+", "-");
            RegisterOperators(3, "<<", ">>");
            RegisterOperators(4, "|", "&", "^");
            RegisterOperators(5, "&&", "||");

            RegisterBracePair("(", ")");



            #endregion

            var program = new NonTerminal("program");
            var function = new NonTerminal("function");
            var declarations = new NonTerminal("declarations");
            var declaration = new NonTerminal("declaration");

            var include = new NonTerminal("include");
            var define = new NonTerminal("define");
            var gscForFunction = new NonTerminal("gscForFunction");
            var baseCall = new NonTerminal("baseCall");
            var baseCallPointer = new NonTerminal("baseCallPointer");
            var scriptFunctionCall = new NonTerminal("scriptFunctionCall");
            var scriptFunctionCallPointer = new NonTerminal("scriptFunctionCallPointer");
            var scriptMethodCall = new NonTerminal("scriptMethodCall");
            var scriptMethodCallPointer = new NonTerminal("scriptMethodCallPointer");
            var scriptThreadCall = new NonTerminal("scriptThreadCall");
            var scriptThreadCallPointer = new NonTerminal("scriptThreadCallPointer");
            var scriptMethodThreadCall = new NonTerminal("scriptMethodThreadCall");
            var scriptMethodThreadCallPointer = new NonTerminal("scriptMethodThreadCallPointer");
            var CallBuiltin = new NonTerminal("CallBuiltin");
            var CallBuiltinMethod = new NonTerminal("CallBuiltinMethod");
            var call = new NonTerminal("call");
            var simpleCall = new NonTerminal("simpleCall");
            var parenParameters = new NonTerminal("parenParameters");
            var parameters = new NonTerminal("parameters");
            var expr = new NonTerminal("expr");
            var setVariableField = new NonTerminal("setVariableField");
            var array = new NonTerminal("array");
            var vector = new NonTerminal("vector");
            var mathOperator = new NonTerminal("mathOperator");
            var relationalOperator = new NonTerminal("relationalOperator");
            var expression = new NonTerminal("expression");
            var directAccess = new NonTerminal("directAccess");
            var boolNot = new NonTerminal("boolNot");
            var wait = new NonTerminal("wait");
            var waittillframeend = new NonTerminal("waittillframeend");
            var size = new NonTerminal("size");
            var isString = new NonTerminal("isString");
            var hashedString = new NonTerminal("hashedString");

            var statement = new NonTerminal("statement");
            var ifStatement = new NonTerminal("ifStatement");
            var elseStatement = new NonTerminal("elseStatement");

            var whileStatement = new NonTerminal("whileStatement");
            var forStatement = new NonTerminal("forStatement");
            var forBody = new NonTerminal("forBody");
            var switchStatement = new NonTerminal("switchStatement");
            var switchLabel = new NonTerminal("switchLabel");
            var switchContents = new NonTerminal("switchContents");
            var switchContent = new NonTerminal("switchContent");
            var foreachStatement = new NonTerminal("foreachStatement");
            var block = new NonTerminal("block");
            var blockContent = new NonTerminal("blockContent");
            var statementBlock = new NonTerminal("statementBlock");
            var shortExprOperator = new NonTerminal("shortExprOperator");
            var forIterate = new NonTerminal("forIterate");
            var conditionalStatement = new NonTerminal("conditionalStatement");
            var _return = new NonTerminal("return");
            var getFunction = new NonTerminal("getFunction");
            var animTree = new NonTerminal("animTree");
            var usingAnimTree = new NonTerminal("usingAnimTree");
            var getAnimation = new NonTerminal("getAnimation");
            var jumpStatement = new NonTerminal("jumpStatement");
            var parenExpr = new NonTerminal("parenExpr");
            var GSCNamespace = new NonTerminal("GSCNamespace");
            var GSCPath = new NonTerminal("GSCPath");
            var GSCChecksum = new NonTerminal("GSCChecksum");

            var FunctionFlag = new NonTerminal("FunctionFlag");
            var NewArray = new NonTerminal("NewArray");
            var NewArrayRule = new NonTerminal("NewArrayRule");
            var booleanOperator = new NonTerminal("booleanOperator");
            var _operator = new NonTerminal("operator");

            var FuncParenParameters = new NonTerminal("FuncParenParameters");
            var FuncParameters = new NonTerminal("FuncParameters");
            var DefaultParameters = new NonTerminal("DefaultParameters");

            GSCNamespace.Rule = ToTerm("#namespace") + pathIdentifier + ";";
            GSCPath.Rule = ToTerm("#path") + pathIdentifier + ";";
            GSCChecksum.Rule = ToTerm("#checksum") + stringLiteral + ";";

            Root = program;

            program.Rule = MakePlusRule(program, include | define | usingAnimTree | GSCPath | GSCNamespace | GSCChecksum | function);

            include.Rule = ToTerm("#include") + pathIdentifier + ";";

            define.Rule = ToTerm("#define") + expr + "=" + expr + ";";

            function.Rule = identifier + FuncParenParameters + block | FunctionFlag + identifier + FuncParenParameters + block; //function structure rule

            usingAnimTree.Rule = ToTerm("#using_animtree") + "(" + stringLiteral + ")" + ";";
            getAnimation.Rule = ToTerm("%") + identifier;
            animTree.Rule = ToTerm("#animtree");

            expr.Rule = conditionalStatement | call | identifier | stringLiteral | array | numberLiteral | vector | directAccess |
            expression | size | boolNot | isString | getFunction | hashedString | getAnimation | animTree | parenExpr | NewArray;

            FunctionFlag.Rule = ToTerm("autoexec");

            parameters.Rule = MakeStarRule(parameters, ToTerm(","), expr) | expr; //paramters for any call rule
            parenParameters.Rule = ToTerm("(") + parameters + ")" | "(" + ")";


            FuncParenParameters.Rule = ToTerm("(") + FuncParameters + ")" | "(" + ")";

            FuncParameters.Rule = MakeStarRule(FuncParameters, ToTerm(","), DefaultParameters | expr) | DefaultParameters | expr; //paramters for any function

            DefaultParameters.Rule = expr + "=" + expr;




            declaration.Rule = waittillframeend | simpleCall | statement | setVariableField | wait | _return | jumpStatement;
            declarations.Rule = MakePlusRule(declarations, declaration);
            block.Rule = ToTerm("{") + blockContent + "}" | ToTerm("{") + "}";
            blockContent.Rule = declarations;
            parenExpr.Rule = ToTerm("(") + expr + ")";





            NewArrayRule.Rule = MakeStarRule(NewArrayRule, ToTerm(","), expr) | expr;

            NewArray.Rule = "[" + NewArrayRule + "]";

            array.Rule = expr + "[" + expr + "]" | ToTerm("[]");
            vector.Rule = ToTerm("(") + expr + "," + expr + "," + expr + ")";
            shortExprOperator.Rule = ToTerm("=") | "+=" | "-=" | "*=" | "/=" | "%=" | "&=" | "|=" | "^=" | "++" | "--";
            setVariableField.Rule = expr + shortExprOperator + expr + ";" | expr + shortExprOperator + ";";


            mathOperator.Rule = ToTerm("+") | "-" | "/" | "*" | "%" | "&" | "|" | "^" | "<<" | ">>";
            relationalOperator.Rule = ToTerm(">") | ">=" | "<" | "<=" | "==" | "!=" | "===" | "!==";
            booleanOperator.Rule = ToTerm("&&") | "||";

            _operator.Rule = mathOperator | relationalOperator | booleanOperator;

            expression.Rule = expr + _operator + expr;

            boolNot.Rule = ToTerm("!") + expr;

            directAccess.Rule = expr + "." + identifier;
            wait.Rule = ToTerm("wait") + expr + ";";
            waittillframeend.Rule = ToTerm("waittillframeend") + ";";
            size.Rule = expr + ".size";
            _return.Rule = ToTerm("return") + expr + ";" | ToTerm("return") + ";";

            jumpStatement.Rule = ToTerm("break") + ";" | ToTerm("continue") + ";"
                | ToTerm("break") + numberLiteral + ";" | ToTerm("continue") + numberLiteral + ";";

            isString.Rule = ToTerm("&") + stringLiteral;
            hashedString.Rule = ToTerm("#") + stringLiteral;
            getFunction.Rule = ToTerm("::") + identifier | gscForFunction + identifier;

            gscForFunction.Rule = identifier + "::";
            baseCall.Rule = gscForFunction + identifier + parenParameters
                | identifier + parenParameters;

            baseCallPointer.Rule = ToTerm("[[") + expr + "]]" + parenParameters;
            CallBuiltin.Rule = ToTerm(":::") + baseCall;
            CallBuiltinMethod.Rule = expr + ToTerm(":::") + baseCall;
            scriptFunctionCall.Rule = baseCall;
            scriptFunctionCallPointer.Rule = baseCallPointer;
            scriptMethodCall.Rule = expr + baseCall;
            scriptMethodCallPointer.Rule = expr + baseCallPointer;
            scriptThreadCall.Rule = ToTerm("thread") + baseCall;
            scriptThreadCallPointer.Rule = ToTerm("thread") + baseCallPointer;
            scriptMethodThreadCall.Rule = expr + "thread" + baseCall;
            scriptMethodThreadCallPointer.Rule = expr + "thread" + baseCallPointer;

            call.Rule = scriptFunctionCall | scriptFunctionCallPointer | scriptMethodCall | scriptMethodCallPointer |
                        scriptThreadCall | scriptThreadCallPointer | scriptMethodThreadCall |
                        scriptMethodThreadCallPointer | CallBuiltin | CallBuiltinMethod;
            simpleCall.Rule = call + ";";

            statementBlock.Rule = block | declaration;
            statement.Rule = ifStatement | whileStatement | forStatement | switchStatement | foreachStatement;

            ifStatement.Rule = ToTerm("if") + "(" + expr + ")" + statementBlock
                | ToTerm("if") + "(" + expr + ")" + statementBlock + elseStatement;
            elseStatement.Rule = ToTerm("else") + statementBlock | ToTerm("else") + ifStatement;

            whileStatement.Rule = ToTerm("while") + "(" + expr + ")" + statementBlock;


            forIterate.Rule = expr + shortExprOperator + expr | expr + shortExprOperator;

            forBody.Rule = setVariableField + expr + ";" + forIterate
                           | ToTerm(";") + expr + ";" + forIterate
                           | ToTerm(";") + ";" + forIterate
                           | ToTerm(";") + ";"
                           | setVariableField + ";" + forIterate
                           | setVariableField + ";"
                           | ToTerm(";") + expr + ";"
                           | setVariableField + expr + ";";

            forStatement.Rule = ToTerm("for") + "(" + forBody + ")" + statementBlock;


            foreachStatement.Rule = ToTerm("foreach") + "(" + identifier + "in" + expr + ")" + statementBlock //foreach(val in array)
                | ToTerm("foreach") + "(" + identifier + "," + identifier + "in" + expr + ")" + statementBlock;//foreach(index, val in array)
            switchLabel.Rule = ToTerm("case") + expr + ":" | ToTerm("default") + ":";
            switchContents.Rule = MakePlusRule(switchContents, switchContent);
            switchContent.Rule = switchLabel + blockContent | switchLabel + block | switchLabel;
            switchStatement.Rule = ToTerm("switch") + parenExpr + "{" + switchContents + "}";
            conditionalStatement.Rule = expr + "?" + expr + ":" + expr;
        }
    }
}
