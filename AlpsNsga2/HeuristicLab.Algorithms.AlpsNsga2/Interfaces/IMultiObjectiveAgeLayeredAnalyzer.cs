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
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;

namespace HeuristicLab.Algorithms.AlpsNsga2.Interfaces
{
    public interface IMultiObjectiveAgeLayeredAnalyzer : IAnalyzer, IMultiObjectiveOperator
    {
        IScopeTreeLookupParameter<DoubleArray> QualitiesParameter { get; }
        IScopeTreeLookupParameter<DoubleValue> CrowdingDistanceParameter { get; }
        IScopeTreeLookupParameter<IntValue> RankParameter { get; }
        IScopeTreeLookupParameter<DoubleValue> AgeParameter { get; }
        ILookupParameter<ResultCollection> ResultsParameter { get; }
    }
}