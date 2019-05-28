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
using HeuristicLab.Algorithms.ALPS;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Optimization.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Selection;

namespace HeuristicLab.Algorithms.AlpsNsga2
{
    [Item("AlpsNsga2MainOperator", "An operator that represents the core of an NSGA-II with ALPS.")]
    [StorableClass]
    public sealed class AlpsNsga2MainOperator : AlgorithmOperator
    {
        #region Parameter properties

        public IValueLookupParameter<IRandom> RandomParameter => (IValueLookupParameter<IRandom>)Parameters["Random"];

        public IValueLookupParameter<IOperator> EvaluatorParameter => (IValueLookupParameter<IOperator>)Parameters["Evaluator"];

        public IValueLookupParameter<IntValue> EvaluatedSolutionsParameter => (IValueLookupParameter<IntValue>)Parameters["EvaluatedSolutions"];

        public IScopeTreeLookupParameter<DoubleValue> QualityParameter => (IScopeTreeLookupParameter<DoubleValue>)Parameters["Quality"];

        public IValueLookupParameter<BoolValue> MaximizationParameter => (IValueLookupParameter<BoolValue>)Parameters["Maximization"];

        public IValueLookupParameter<IntValue> PopulationSizeParameter => (IValueLookupParameter<IntValue>)Parameters["PopulationSize"];

        public IValueLookupParameter<IOperator> SelectorParameter => (IValueLookupParameter<IOperator>)Parameters["Selector"];

        public ValueLookupParameter<PercentValue> CrossoverProbabilityParameter => (ValueLookupParameter<PercentValue>)Parameters["CrossoverProbability"];

        public IValueLookupParameter<IOperator> CrossoverParameter => (IValueLookupParameter<IOperator>)Parameters["Crossover"];

        public IValueLookupParameter<IOperator> MutatorParameter => (IValueLookupParameter<IOperator>)Parameters["Mutator"];

        public IValueLookupParameter<PercentValue> MutationProbabilityParameter => (IValueLookupParameter<PercentValue>)Parameters["MutationProbability"];

        public IScopeTreeLookupParameter<DoubleValue> AgeParameter => (IScopeTreeLookupParameter<DoubleValue>)Parameters["Age"];

        public IValueLookupParameter<DoubleValue> AgeInheritanceParameter => (IValueLookupParameter<DoubleValue>)Parameters["AgeInheritance"];

        public IValueLookupParameter<DoubleValue> AgeIncrementParameter => (IValueLookupParameter<DoubleValue>)Parameters["AgeIncrement"];

        public IValueLookupParameter<BoolValue> DominateOnEqualQualitiesParameter => (ValueLookupParameter<BoolValue>)Parameters["DominateOnEqualQualities"];

        #endregion

        [StorableConstructor]
        private AlpsNsga2MainOperator(bool deserializing) : base(deserializing) { }
        private AlpsNsga2MainOperator(AlpsNsga2MainOperator original, Cloner cloner)
          : base(original, cloner)
        {
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new AlpsNsga2MainOperator(this, cloner);
        }

        public AlpsNsga2MainOperator() : base()
        {
            Parameters.Add(new ValueLookupParameter<IRandom>("Random", "A pseudo random number generator."));

            Parameters.Add(new ValueLookupParameter<IOperator>("Evaluator", "The operator used to evaluate solutions. This operator is executed in parallel, if an engine is used which supports parallelization."));
            Parameters.Add(new ValueLookupParameter<IntValue>("EvaluatedSolutions", "The number of times solutions have been evaluated."));
            Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>("Quality", "The value which represents the quality of a solution."));
            Parameters.Add(new ValueLookupParameter<BoolValue>("Maximization", "True if the problem is a maximization problem, otherwise false."));

            Parameters.Add(new ValueLookupParameter<IntValue>("PopulationSize", "The size of the population of solutions in each layer."));
            Parameters.Add(new ValueLookupParameter<IOperator>("Selector", "The operator used to select solutions for reproduction."));

            Parameters.Add(new ValueLookupParameter<IOperator>("Crossover", "The operator used to cross solutions."));
            Parameters.Add(new ValueLookupParameter<PercentValue>("CrossoverProbability", "The probability that the crossover operator is applied on a solution."));
            Parameters.Add(new ValueLookupParameter<IOperator>("Mutator", "The operator used to mutate solutions."));
            Parameters.Add(new ValueLookupParameter<PercentValue>("MutationProbability", "The probability that the mutation operator is applied on a solution."));

            Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>("Age", "The age of individuals."));
            Parameters.Add(new ValueLookupParameter<DoubleValue>("AgeInheritance", "A weight that determines the age of a child after crossover based on the older (1.0) and younger (0.0) parent."));
            Parameters.Add(new ValueLookupParameter<DoubleValue>("AgeIncrement", "The value the age the individuals is incremented if they survives a generation."));

            Parameters.Add(new ValueLookupParameter<BoolValue>("DominateOnEqualQualities", "Flag which determines whether solutions with equal quality values should be treated as dominated."));

            var selector = new Placeholder { Name = "Selector (Placeholder)" };
            var subScopesProcessor1 = new SubScopesProcessor();
            var childrenCreator = new ChildrenCreator();
            var uniformSubScopesProcessor1 = new UniformSubScopesProcessor();
            var crossoverStochasticBranch = new StochasticBranch { Name = "CrossoverProbability" };
            var crossover = new Placeholder { Name = "Crossover (Placeholder)" };
            var noCrossover = new ParentCopyCrossover();
            var mutationStochasticBranch = new StochasticBranch { Name = "MutationProbability" };
            var mutator = new Placeholder { Name = "Mutator (Placeholder)" };
            var ageCalculator = new WeightingReducer { Name = "Calculate Age" };
            var subScopesRemover = new SubScopesRemover();
            var uniformSubScopesProcessor2 = new UniformSubScopesProcessor();
            var evaluator = new Placeholder { Name = "Evaluator (Placeholder)" };
            var subScopesCounter = new SubScopesCounter { Name = "Increment EvaluatedSolutions" };
            var mergingReducer = new MergingReducer();
            var rankAndCrowdingSorter1 = new RankAndCrowdingSorter();
            var rankAndCrowdingSorter2 = new RankAndCrowdingSorter();
            var leftSelector = new LeftSelector();
            var rightReducer = new RightReducer();
            var incrementAgeProcessor = new UniformSubScopesProcessor();
            var ageIncrementer = new DoubleCounter { Name = "Increment Age" };

            OperatorGraph.InitialOperator = rankAndCrowdingSorter1;

            rankAndCrowdingSorter1.DominateOnEqualQualitiesParameter.ActualName = DominateOnEqualQualitiesParameter.Name;
            rankAndCrowdingSorter1.CrowdingDistanceParameter.ActualName = "CrowdingDistance";
            rankAndCrowdingSorter1.RankParameter.ActualName = "Rank";
            rankAndCrowdingSorter1.Successor = selector;

            selector.OperatorParameter.ActualName = SelectorParameter.Name;
            selector.Successor = subScopesProcessor1;

            subScopesProcessor1.Operators.Add(new EmptyOperator());
            subScopesProcessor1.Operators.Add(childrenCreator);
            subScopesProcessor1.Successor = mergingReducer; 

            childrenCreator.ParentsPerChild = new IntValue(2);
            childrenCreator.Successor = uniformSubScopesProcessor1;

            uniformSubScopesProcessor1.Operator = crossoverStochasticBranch;
            uniformSubScopesProcessor1.Successor = uniformSubScopesProcessor2;

            crossoverStochasticBranch.ProbabilityParameter.ActualName = CrossoverProbabilityParameter.Name;
            crossoverStochasticBranch.RandomParameter.ActualName = RandomParameter.Name;
            crossoverStochasticBranch.FirstBranch = crossover;
            crossoverStochasticBranch.SecondBranch = noCrossover;
            crossoverStochasticBranch.Successor = mutationStochasticBranch;

            crossover.Name = "Crossover";
            crossover.OperatorParameter.ActualName = CrossoverParameter.Name;
            crossover.Successor = null;

            noCrossover.Name = "Clone parent";
            noCrossover.RandomParameter.ActualName = RandomParameter.Name;
            noCrossover.Successor = null;

            mutationStochasticBranch.ProbabilityParameter.ActualName = MutationProbabilityParameter.Name;
            mutationStochasticBranch.RandomParameter.ActualName = RandomParameter.Name;
            mutationStochasticBranch.FirstBranch = mutator;
            mutationStochasticBranch.SecondBranch = null;
            mutationStochasticBranch.Successor = ageCalculator;

            mutator.Name = "Mutator";
            mutator.OperatorParameter.ActualName = MutatorParameter.Name;
            mutator.Successor = null;

            ageCalculator.ParameterToReduce.ActualName = AgeParameter.Name;
            ageCalculator.TargetParameter.ActualName = AgeParameter.Name;
            ageCalculator.WeightParameter.ActualName = AgeInheritanceParameter.Name;
            ageCalculator.Successor = subScopesRemover;

            subScopesRemover.RemoveAllSubScopes = true;
            subScopesRemover.Successor = null;

            uniformSubScopesProcessor2.Parallel.Value = true;
            uniformSubScopesProcessor2.Operator = evaluator;
            uniformSubScopesProcessor2.Successor = subScopesCounter;

            evaluator.OperatorParameter.ActualName = EvaluatorParameter.Name;
            evaluator.Successor = null;

            subScopesCounter.ValueParameter.ActualName = EvaluatedSolutionsParameter.Name;
            subScopesCounter.AccumulateParameter.Value = new BoolValue(true);
            subScopesCounter.Successor = null;

            mergingReducer.Successor = rankAndCrowdingSorter2;

            rankAndCrowdingSorter2.DominateOnEqualQualitiesParameter.ActualName = DominateOnEqualQualitiesParameter.Name;
            rankAndCrowdingSorter2.CrowdingDistanceParameter.ActualName = "CrowdingDistance";
            rankAndCrowdingSorter2.RankParameter.ActualName = "Rank";
            rankAndCrowdingSorter2.Successor = leftSelector;

            leftSelector.CopySelected = new BoolValue(false);
            leftSelector.NumberOfSelectedSubScopesParameter.ActualName = PopulationSizeParameter.Name;
            leftSelector.Successor = rightReducer;

            rightReducer.Successor = incrementAgeProcessor;
            
            incrementAgeProcessor.Operator = ageIncrementer;
            incrementAgeProcessor.Successor = null;

            ageIncrementer.ValueParameter.ActualName = AgeParameter.Name;
            ageIncrementer.IncrementParameter.Value = null;
            ageIncrementer.IncrementParameter.ActualName = AgeIncrementParameter.Name;
            ageIncrementer.Successor = null;
        }
    }
}
