namespace DefaultDocumentation.Model
{
   internal interface IParameterizedDocItem : IDocItem
    {
        ParameterDocItem[] Parameters { get; }
    }
}
