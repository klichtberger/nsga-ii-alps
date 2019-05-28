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
using System.Collections.Generic;
using HeuristicLab.Analysis;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Algorithms.AlpsNsga2.Analyzers
{
    public class AlpsScatterPlotAnalyzer : MultiObjectiveAgeLayeredAnalyzer
    {
        [StorableConstructor]
        protected AlpsScatterPlotAnalyzer(bool deserializing) : base(deserializing) { }
        protected AlpsScatterPlotAnalyzer(AlpsScatterPlotAnalyzer original, Cloner cloner) : base(original, cloner) { }
        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new AlpsScatterPlotAnalyzer(this, cloner);
        }

        public AlpsScatterPlotAnalyzer() { }

        protected override void Analyze(ItemArray<DoubleArray> qualities, ResultCollection results)
        {
            var ranks = RankParameter.ActualValue;
            var ages = AgeParameter.ActualValue;

            var objectiveCnt = qualities[0].Length;

            if (objectiveCnt == 2)
            {
                var scatterPlot = new ScatterPlot();

                var objectiveAges = new Dictionary<int, List<Point2D<double>>>();
                for (var i = 0; i < ranks.Length; i++)
                {
                    var key = (int) ages[i].Value;

                    if (objectiveAges.ContainsKey(key))
                    {
                        objectiveAges[key].Add(new Point2D<double>(qualities[i][0], qualities[i][1]));
                    }
                    else
                    {
                        objectiveAges.Add(key,
                            new List<Point2D<double>> {new Point2D<double>(qualities[i][0], qualities[i][1])});
                    }
                }

                foreach (var key in objectiveAges.Keys)
                {
                    var dataRow = new ScatterPlotDataRow("Age " + key, "ALPS Pareto Front ScatterPlot",
                        objectiveAges[key]);
                    scatterPlot.Rows.Add(dataRow);
                }

                if (results.ContainsKey("ALPS Pareto Front ScatterPlot"))
                {
                    results["ALPS Pareto Front ScatterPlot"].Value = scatterPlot;
                }
                else results.Add(new Result("ALPS Pareto Front ScatterPlot", scatterPlot));
            }
        }
    }
}