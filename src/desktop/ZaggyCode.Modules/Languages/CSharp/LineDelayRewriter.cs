namespace ZaggyCode.Modules.Languages.CSharp;

//#:NO_AI
public class LineDelayRewriter(int delayMs) : CSharpSyntaxRewriter
{
    private readonly int _delayMs = delayMs;

    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {
        var newMembers = SyntaxFactory.List<MemberDeclarationSyntax>();
        foreach (var member in node.Members)
        {
            newMembers = newMembers.Add((MemberDeclarationSyntax)Visit(member));
            if (member is GlobalStatementSyntax globalStatement)
            {
                if (globalStatement.Statement is ExpressionStatementSyntax)
                {
                    var delayStatement = SyntaxFactory.ParseStatement($"System.Threading.Tasks.Task.Delay({_delayMs}).Wait();\r\n");
                    newMembers = newMembers.Add(SyntaxFactory.GlobalStatement(delayStatement));
                }
            }
        }

        return node.WithMembers(newMembers);
    }
}