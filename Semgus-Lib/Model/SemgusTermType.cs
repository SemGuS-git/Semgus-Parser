﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Semgus.Model.Smt;
using Semgus.Model.Smt.Sorts;

namespace Semgus.Model
{
    public class SemgusTermType : SmtSort
    {
        public SemgusTermType(SmtSortIdentifier termname) : base(termname) { }
        public IList<Constructor> Constructors { get; } = new List<Constructor>();
        public void AddConstructor(Constructor constructor)
        {
            Constructors.Add(constructor);
        }

        public record Constructor(SmtIdentifier Operator, params SmtSort[] Children) : ISmtConstructor
        {
            public SmtIdentifier Name => Operator;

            IReadOnlyList<SmtSort> ISmtConstructor.Children => Children.ToList();
        }
    }
}
