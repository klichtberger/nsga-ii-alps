/* Author: Katharina Lichtberger
 *
 * This implementation uses parts of and from HeuristicLab.
 * Copyright (C) 2002-2018 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
 *
 * HeuristicLab is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
using HeuristicLab.Algorithms.AlpsNsga2.Interfaces;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Algorithms.AlpsNsga2.Analyzers
{
    [StorableClass]
    public abstract class MultiObjectiveAgeLayeredAnalyzer : SingleSuccessorOperator, IMultiObjectiveAgeLayeredAnalyzer
    {
        public virtual bool EnabledByDefault => true;

        public IScopeTreeLookupParameter<DoubleArray> QualitiesParameter => (IScopeTreeLookupParameter<DoubleArray>)Parameters["Qualities"];

        public IScopeTreeLookupParameter<DoubleValue> CrowdingDistanceParameter => (IScopeTreeLookupParameter<DoubleValue>)Parameters["CrowdingDistance"];

        public IScopeTreeLookupParameter<IntValue> RankParameter => (IScopeTreeLookupParameter<IntValue>)Parameters["Rank"];

        public IScopeTreeLookupParameter<DoubleValue> AgeParameter => (IScopeTreeLookupParameter<DoubleValue>)Parameters["Age"];

        public ILookupParameter<ResultCollection> ResultsParameter => (ILookupParameter<ResultCollection>)Parameters["Results"];
        

        protected MultiObjectiveAgeLayeredAnalyzer(MultiObjectiveAgeLayeredAnalyzer original, Cloner cloner) : base(original, cloner) { }

        [StorableConstructor]
        protected MultiObjectiveAgeLayeredAnalyzer(bool deserializing) : base(deserializing) { }

        protected MultiObjectiveAgeLayeredAnalyzer()
        {
            Parameters.Add(new ScopeTreeLookupParameter<DoubleArray>("Qualities", "The qualities of the parameter vector."));
            Parameters.Add(new ScopeTreeLookupParameter<IntValue>("Rank", "The rank of solution [0..N] describes to which front it belongs."));
            Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>("CrowdingDistance", "The crowding distance of the solution."));
            Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>("Age", "The age of individuals."));
            Parameters.Add(new LookupParameter<ResultCollection>("Results", "The results collection to write to."));
        }

        public override IOperation Apply()
        {
            ItemArray<DoubleArray> qualities = QualitiesParameter.ActualValue;
            ResultCollection results = ResultsParameter.ActualValue;

            Analyze(qualities, results);
            return base.Apply();
        }

        protected abstract void Analyze(ItemArray<DoubleArray> qualities, ResultCollection results);
    }
}
