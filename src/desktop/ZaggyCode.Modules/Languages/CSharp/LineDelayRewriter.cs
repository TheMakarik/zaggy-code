namespace ZaggyCode.Modules.Languages.CSharp;

public class LineDelayRewriter(int delayMs) : CSharpSyntaxRewriter
{
    private readonly int _delayMs = delayMs;

    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {
        SyntaxList<MemberDeclarationSyntax> newMembers = SyntaxFactory.List<MemberDeclarationSyntax>();
        foreach (MemberDeclarationSyntax member in node.Members)
        {
            newMembers = newMembers.Add((MemberDeclarationSyntax)Visit(member));
            if (member is GlobalStatementSyntax globalStatement)
            {
                if (globalStatement.Statement is ExpressionStatementSyntax)
                {
                    StatementSyntax delayStatement = SyntaxFactory.ParseStatement($"System.Threading.Tasks.Task.Delay({_delayMs}).Wait();\r\n");
                    newMembers = newMembers.Add(SyntaxFactory.GlobalStatement(delayStatement));
                }
            }
        }

        return node.WithMembers(newMembers);
    }
}