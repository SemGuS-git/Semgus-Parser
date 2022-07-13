using Semgus.Model.Smt;
using Semgus.Model.Smt.Terms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Semgus.Model
{
    public class SemgusContext
    {
        private readonly List<SmtTerm> _constraints = new();
        private readonly List<SemgusChc> _chcs = new();
        private readonly List<SemgusSynthFun> _synthFun = new();

        private SemgusChc.SemanticRelation? _sygusRelation;
        private string? _sygusLogic = default;
        private readonly List<(SmtIdentifier, SmtSort)> _sygusVars = new();
        private readonly List<(SmtFunction, SmtFunction, SmtFunction, SmtFunctionRank)> _sygusSynthFuns = new();

        public void AddConstraint(SmtTerm constraint)
        {
            _constraints.Add(constraint);
        }

        public void AddChc(SemgusChc chc)
        {
            _chcs.Add(chc);
        }

        public void AddSynthFun(SemgusSynthFun sf)
        {
            _synthFun.Add(sf);
        }

        public void SetSygusLogic(string logic)
        {
            _sygusLogic = logic;
        }

        public void SetSygusRelation(SemgusChc.SemanticRelation relation)
        {
            _sygusRelation = relation;
        }

        public void AddSygusVar(SmtIdentifier sygusVar, SmtSort sort)
        {
            _sygusVars.Add((sygusVar, sort));
        }

        public void AddSygusSynthFun(SmtFunction semTerm, SmtFunction semRel, SmtFunction sygFun, SmtFunctionRank sygRank)
        {
            _sygusSynthFuns.Add((semTerm, semRel, sygFun, sygRank));
        }

        public IReadOnlyCollection<SemgusChc> Chcs => _chcs;
        public IReadOnlyCollection<SmtTerm> Constraints => _constraints;
        public IReadOnlyCollection<SemgusSynthFun> SynthFuns => _synthFun;
        public string? SygusLogic => _sygusLogic;
        public SemgusChc.SemanticRelation? SygusRelation => _sygusRelation;
        public IReadOnlyCollection<(SmtIdentifier, SmtSort)> SygusVars => _sygusVars;
        public IReadOnlyCollection<(SmtFunction SemTerm, SmtFunction SemRel, SmtFunction SygFun, SmtFunctionRank SygRank)> SygusSynthFuns => _sygusSynthFuns;

        public bool AnySygusFeatures => _sygusLogic is not null
                                     || _sygusRelation is not null
                                     || _sygusVars.Count > 0
                                     || _sygusSynthFuns.Count > 0;
    }
}
