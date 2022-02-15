namespace Semgus.Model.Smt
{
    public record SmtVariableBinding(SmtIdentifier Id, SmtSort Sort, SmtVariableBindingType BindingType, SmtScope DeclaringScope);

    public enum SmtVariableBindingType
    {
        Free,
        Bound,
        Existential,
        Universal,
        Lambda
    }
}
