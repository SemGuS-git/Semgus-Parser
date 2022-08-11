using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Semgus.Parser.Json.Converters;
using System.IO;

namespace Semgus.Parser.Json
{
    internal class JsonHandler : ISemgusProblemHandler, IDisposable
    {
        private readonly JsonSerializer _serializer;
        private readonly TextWriter _writer;
        private readonly Program.ProcessingMode _processingMode;

        public JsonHandler(TextWriter writer, Program.ProcessingMode mode)
        {
            _serializer = new JsonSerializer();
            _serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
            _serializer.Converters.Add(new SmtIdentifierConverter());
            _serializer.Converters.Add(new SemanticRelationConverter());
            _serializer.Converters.Add(new SmtTermConverter());
            _serializer.Converters.Add(new SmtAttributeValueConverter());
            _serializer.Converters.Add(new SmtSortIdentifierConverter());
            _writer = writer;
            _processingMode = mode;
            if (_processingMode == Program.ProcessingMode.Batch)
            {
                _writer.WriteLine("[");
            }
        }

        public void Dispose()
        {
            _writer.WriteLine(@"{""$type"":""meta"",""$event"":""end-of-stream""}");
            if (_processingMode == Program.ProcessingMode.Batch)
            {
                _writer.WriteLine("]");
            }
            _writer.Flush();
        }

        private void EndOfEvent()
        {
            if (_processingMode == Program.ProcessingMode.Batch)
            {
                _writer.WriteLine(",");
            }
            else
            {
                _writer.WriteLine();
            }
        }

        public void OnCheckSynth(SmtContext smtCtx, SemgusContext semgusCtx)
        {
            foreach (var chc in semgusCtx.Chcs)
            {
                _serializer.Serialize(_writer, new ChcEvent(chc));
                EndOfEvent();
            }

            foreach (var ssf in semgusCtx.SynthFuns)
            {
                _serializer.Serialize(_writer, new SynthFunEvent(ssf));
                EndOfEvent();
            }

            foreach (var c in semgusCtx.Constraints)
            {
                _serializer.Serialize(_writer, new ConstraintEvent(c));
                EndOfEvent();
            }

            _serializer.Serialize(_writer, new CheckSynthEvent());
            EndOfEvent();
        }

        public void OnConstraint(SmtContext smtCtx, SemgusContext semgusCxt, SmtTerm constraint)
        {
        }

        public void OnSetInfo(SmtContext ctx, SmtAttribute attr)
        {
            _serializer.Serialize(_writer, new SetInfoEvent(attr));
            EndOfEvent();
        }

        public void OnSynthFun(SmtContext ctx, SmtIdentifier name, IList<(SmtIdentifier, SmtSortIdentifier)> args, SmtSort sort)
        {
        }

        public void OnTermTypes(IReadOnlyList<SemgusTermType> termTypes)
        {
            // First, the declarations, so the consumer knows what term types exist
            foreach (var tt in termTypes)
            {
                _serializer.Serialize(_writer, new TermTypeDeclarationEvent(tt));
                EndOfEvent();
            }

            // Then the definitions, which can reference the previous declarations
            foreach (var tt in termTypes)
            {
                _serializer.Serialize(_writer, new TermTypeDefinitionEvent(tt));
                EndOfEvent();
            }
        }
    }
}
