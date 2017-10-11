
namespace Fireasy.Data
{
    public class QueryCommand
    {
        private IQueryCommand command;
        private ParameterCollection parameters;

        public QueryCommand(IQueryCommand command, ParameterCollection parameters)
        {
            this.command = command;
            this.parameters = parameters;
        }

        public IQueryCommand Command
        {
            get { return command; }
        }

        public ParameterCollection Parameters
        {
            get { return this.parameters; }
        }
    }
}
