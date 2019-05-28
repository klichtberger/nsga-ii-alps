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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Algorithms.AlpsNsga2.Analyzers
{
    public class ParetoFrontAlpsAnalyzer : MultiObjectiveAgeLayeredAnalyzer
    {
        [StorableConstructor]
        protected ParetoFrontAlpsAnalyzer(bool deserializing) : base(deserializing) { }
        protected ParetoFrontAlpsAnalyzer(ParetoFrontAlpsAnalyzer original, Cloner cloner) : base(original, cloner) { }
        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new ParetoFrontAlpsAnalyzer(this, cloner);
        }

        public ParetoFrontAlpsAnalyzer() { }

        protected override void Analyze(ItemArray<DoubleArray> qualities, ResultCollection results)
        {
            var ranks = RankParameter.ActualValue;
            var ages = AgeParameter.ActualValue;
            var crowdingDistance = CrowdingDistanceParameter.ActualValue;
            
            var objectiveCnt = qualities[0].Length;
            var frontSize = ranks.Length;

            // Columns for objectives, rank, age and crowding distance
            var front = new DoubleMatrix(frontSize, objectiveCnt + 3);

            var counter = 0;
            for (var i = 0; i < ranks.Length; i++)
            {
                for (var k = 0; k < objectiveCnt; k++)
                {
                    front[counter, k] = qualities[i][k];
                }

                front[counter, objectiveCnt] = ranks[i].Value;
                front[counter, objectiveCnt + 1] = crowdingDistance[i].Value;
                front[counter, objectiveCnt + 2] = ages[i].Value;

                counter++;
            }

            front.RowNames = GetRowNames(front);
            front.ColumnNames = GetColumnNames(objectiveCnt, front);

            if (results.ContainsKey("ALPS Pareto Front"))
                results["ALPS Pareto Front"].Value = front;
            else results.Add(new Result("ALPS Pareto Front", front));
        }


        private IEnumerable<string> GetRowNames(DoubleMatrix front)
        {
            for (var i = 1; i <= front.Rows; i++)
                yield return "Solution " + i;
        }

        private IEnumerable<string> GetColumnNames(int objectives, DoubleMatrix front)
        {
            for (var i = 1; i <= front.Columns; i++)
            {
                if (i <= objectives)
                {
                    yield return "Objective " + i;
                }
                else if (i <= objectives + 1)
                {
                    yield return "Rank";
                }
                else if (i <= objectives + 2)
                {
                    yield return "Crowding Distance";
                }
                else yield return "Age";
            }
        }
    }
}