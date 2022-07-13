using Microsoft.Extensions.Logging;

using Semgus.Model;
using Semgus.Model.Smt;
using Semgus.Parser.Reader;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Parser.Commands.Sygus
{
    /// <summary>
    /// Creates term types for Sygus problems
    /// </summary>
    internal class SygusTermTypeBuilder
    {
        private readonly SmtContext _ctx;
        private readonly ISourceMap _sourceMap;
        public SygusTermTypeBuilder(SmtContext ctx, ISourceMap sourceMap)
        {
            _ctx = ctx;
            _sourceMap = sourceMap;
        }

        public void BuildTermTypes<T>(IEnumerable<(SmtIdentifier, SmtSortIdentifier)> arguments, SmtSortIdentifier ret, ILogger<T> logger)
        {
            var intSort = _ctx.GetSortOrDie(SmtCommonIdentifiers.IntSortId, _sourceMap, logger);
            var boolSort = _ctx.GetSortOrDie(SmtCommonIdentifiers.BoolSortId, _sourceMap, logger);

            var intTT = new SemgusTermType(new(GensymUtils.Gensym("_Sy", "Int")));
            var boolTT = new SemgusTermType(new(GensymUtils.Gensym("_Sy", "Bool")));
            var stringTT = new SemgusTermType(new(GensymUtils.Gensym("_Sy", "String")));

            intTT.AddConstructor(new(new("-"), intTT));
            intTT.AddConstructor(new(new("-"), intTT, intTT));
            intTT.AddConstructor(new(new("+"), intTT, intTT));
            intTT.AddConstructor(new(new("*"), intTT, intTT));
            intTT.AddConstructor(new(new("div"), intTT, intTT));
            intTT.AddConstructor(new(new("mod"), intTT, intTT));
            intTT.AddConstructor(new(new("abs"), intTT));
            intTT.AddConstructor(new(new("ite"), boolTT, intTT, intTT));
            intTT.AddConstructor(new(new("constant", new SmtIdentifier.Index("Int")), intSort));

            boolTT.AddConstructor(new(new("="), intTT, intTT));
            boolTT.AddConstructor(new(new("<"), intTT, intTT));
            boolTT.AddConstructor(new(new(">"), intTT, intTT));
            boolTT.AddConstructor(new(new("<="), intTT, intTT));
            boolTT.AddConstructor(new(new(">="), intTT, intTT));
            intTT.AddConstructor(new(new("constant", new SmtIdentifier.Index("Bool")), intSort));

            foreach (var (id, sort) in arguments)
            {
                if (sort == SmtCommonIdentifiers.IntSortId)
                {
                    intTT.AddConstructor(new(id));
                }
                else if (sort == SmtCommonIdentifiers.BoolSortId)
                {
                    boolTT.AddConstructor(new(id));
                }
            }
        }
    }
}
