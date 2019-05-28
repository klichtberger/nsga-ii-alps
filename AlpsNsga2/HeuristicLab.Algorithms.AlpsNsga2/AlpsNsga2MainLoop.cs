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
using HeuristicLab.Optimization;
using HeuristicLab.Optimization.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Selection;

namespace HeuristicLab.Algorithms.AlpsNsga2
{
    public class AlpsNsga2MainLoop : AlgorithmOperator
    {
        #region Parameter properties

        public IValueLookupParameter<IRandom> GlobalRandomParameter => (IValueLookupParameter<IRandom>)Parameters["GlobalRandom"];

        public IValueLookupParameter<IRandom> LocalRandomParameter => (IValueLookupParameter<IRandom>)Parameters["LocalRandom"];

        public ValueLookupParameter<BoolArray> MaximizationParameter => (ValueLookupParameter<BoolArray>)Parameters["Maximization"];

        public ScopeTreeLookupParameter<DoubleArray> QualitiesParameter => (ScopeTreeLookupParameter<DoubleArray>)Parameters["Qualities"];

        public ValueLookupParameter<IntValue> PopulationSizeParameter => (ValueLookupParameter<IntValue>)Parameters["PopulationSize"];

        public ILookupParameter<IntValue> CurrentPopulationSizeParameter => (ILookupParameter<IntValue>)Parameters["CurrentPopulationSize"];

        public ValueLookupParameter<IOperator> SelectorParameter => (ValueLookupParameter<IOperator>)Parameters["Selector"];

        public ValueLookupParameter<PercentValue> CrossoverProbabilityParameter => (ValueLookupParameter<PercentValue>)Parameters["CrossoverProbability"];

        public ValueLookupParameter<IOperator> CrossoverParameter => (ValueLookupParameter<IOperator>)Parameters["Crossover"];

        public ValueLookupParameter<PercentValue> MutationProbabilityParameter => (ValueLookupParameter<PercentValue>)Parameters["MutationProbability"];

        public ValueLookupParameter<IOperator> MutatorParameter => (ValueLookupParameter<IOperator>)Parameters["Mutator"];

        public ValueLookupParameter<IOperator> EvaluatorParameter => (ValueLookupParameter<IOperator>)Parameters["Evaluator"];

        public IScopeTreeLookupParameter<DoubleValue> AgeParameter => (IScopeTreeLookupParameter<DoubleValue>)Parameters["Age"];

        public IValueLookupParameter<IntValue> AgeGapParameter => (IValueLookupParameter<IntValue>)Parameters["AgeGap"];

        public IValueLookupParameter<DoubleValue> AgeInheritanceParameter => (IValueLookupParameter<DoubleValue>)Parameters["AgeInheritance"];

        public IValueLookupParameter<IntArray> AgeLimitsParameter => (IValueLookupParameter<IntArray>)Parameters["AgeLimits"];

        public IValueLookupParameter<IntValue> MatingPoolRangeParameter => (IValueLookupParameter<IntValue>)Parameters["MatingPoolRange"];

        public IValueLookupParameter<BoolValue> ReduceToPopulationSizeParameter => (IValueLookupParameter<BoolValue>)Parameters["ReduceToPopulationSize"];

        public IValueLookupParameter<IOperator> TerminatorParameter => (IValueLookupParameter<IOperator>)Parameters["Terminator"];

        public ValueLookupParameter<VariableCollection> ResultsParameter => (ValueLookupParameter<VariableCollection>)Parameters["Results"];

        public ValueLookupParameter<IOperator> AnalyzerParameter => (ValueLookupParameter<IOperator>)Parameters["Analyzer"];

        public ILookupParameter<IOperator> LayerAnalyzerParameter => (ILookupParameter<IOperator>)Parameters["LayerAnalyzer"];

        public ILookupParameter<IOperator> FinalAnalyzerParameter => (ILookupParameter<IOperator>)Parameters["FinalAnalyzer"];

        public IValueLookupParameter<IntValue> NumberOfLayersParameter => (IValueLookupParameter<IntValue>)Parameters["NumberOfLayers"];

        public LookupParameter<IntValue> EvaluatedSolutionsParameter => (LookupParameter<IntValue>)Parameters["EvaluatedSolutions"];

        public IValueLookupParameter<BoolValue> DominateOnEqualQualitiesParameter => (ValueLookupParameter<BoolValue>)Parameters["DominateOnEqualQualities"];

        public FixedValueParameter<IntValue> ZeroParameter => (FixedValueParameter<IntValue>) Parameters["Zero"];

        public FixedValueParameter<IntValue> OneParameter => (FixedValueParameter<IntValue>)Parameters["One"];

        #endregion

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new AlpsNsga2MainLoop(this, cloner);
        }

        [StorableConstructor]
        protected AlpsNsga2MainLoop(bool deserializing) : base(deserializing) { }

        [StorableHook(HookType.AfterDeserialization)]
        private void AfterDeserialization()
        {
            // BackwardsCompatibility3.3
            #region Backwards compatible code, remove with 3.4
            if (!Parameters.ContainsKey("DominateOnEqualQualities"))
                Parameters.Add(new ValueLookupParameter<BoolValue>("DominateOnEqualQualities", "Flag which determines whether solutions with equal quality values should be treated as dominated."));
            #endregion
        }

        protected AlpsNsga2MainLoop(AlpsNsga2MainLoop original, Cloner cloner) : base(original, cloner) { }
        public AlpsNsga2MainLoop() : base()
        {
            Initialize();
        }

        private void Initialize()
        {
            #region Create parameters

            Parameters.Add(new ValueLookupParameter<IRandom>("GlobalRandom", "A pseudo random number generator."));
            Parameters.Add(new ValueLookupParameter<IRandom>("LocalRandom", "A pseudo random number generator."));

            Parameters.Add(new ValueLookupParameter<BoolArray>("Maximization", "True if an objective should be maximized, or false if it should be minimized."));
            Parameters.Add(new ScopeTreeLookupParameter<DoubleArray>("Qualities", "The vector of quality values."));

            Parameters.Add(new ValueLookupParameter<IntValue>("NumberOfLayers", "The number of layers."));
            Parameters.Add(new ValueLookupParameter<IntValue>("PopulationSize", "The population size."));
            Parameters.Add(new LookupParameter<IntValue>("CurrentPopulationSize", "The current size of the population."));

            Parameters.Add(new ValueLookupParameter<IOperator>("Selector", "The operator used to select solutions for reproduction."));

            Parameters.Add(new ValueLookupParameter<PercentValue>("CrossoverProbability", "The probability that the crossover operator is applied on a solution."));
            Parameters.Add(new ValueLookupParameter<IOperator>("Crossover", "The operator used to cross solutions."));
            Parameters.Add(new ValueLookupParameter<PercentValue>("MutationProbability", "The probability that the mutation operator is applied on a solution."));
            Parameters.Add(new ValueLookupParameter<IOperator>("Mutator", "The operator used to mutate solutions."));

            Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>("Age", "The age of individuals."));
            Parameters.Add(new ValueLookupParameter<IntValue>("AgeGap", "The frequency of reseeding the lowest layer and scaling factor for the age-limits for the layers."));
            Parameters.Add(new ValueLookupParameter<DoubleValue>("AgeInheritance", "A weight that determines the age of a child after crossover based on the older (1.0) and younger (0.0) parent."));
            Parameters.Add(new ValueLookupParameter<IntArray>("AgeLimits", "The maximum age an individual is allowed to reach in a certain layer."));

            Parameters.Add(new ValueLookupParameter<IntValue>("MatingPoolRange", "The range of sub - populations used for creating a mating pool. (1 = current + previous sub-population)"));
            Parameters.Add(new ValueLookupParameter<BoolValue>("ReduceToPopulationSize", "Reduce the CurrentPopulationSize after elder migration to PopulationSize"));

            Parameters.Add(new ValueLookupParameter<IOperator>("Evaluator", "The operator used to evaluate solutions. This operator is executed in parallel, if an engine is used which supports parallelization."));
            Parameters.Add(new ValueLookupParameter<VariableCollection>("Results", "The variable collection where results should be stored."));

            Parameters.Add(new ValueLookupParameter<IOperator>("Analyzer", "The operator used to analyze each generation."));
            Parameters.Add(new ValueLookupParameter<IOperator>("LayerAnalyzer", "The operator used to analyze each layer."));
            Parameters.Add(new ValueLookupParameter<IOperator>("FinalAnalyzer", "The operator used to finally analyze the solution (after termination of the algorithm)."));

            Parameters.Add(new LookupParameter<IntValue>("EvaluatedSolutions", "The number of times solutions have been evaluated."));
            Parameters.Add(new ValueLookupParameter<BoolValue>("DominateOnEqualQualities", "Flag which determines whether solutions with equal quality values should be treated as dominated."));

            Parameters.Add(new ValueLookupParameter<IOperator>("Terminator", "The termination criteria that defines if the algorithm should continue or stop"));

            Parameters.Add(new FixedValueParameter<IntValue>("Zero", "Zero Value.", new IntValue(0)));
            Parameters.Add(new FixedValueParameter<IntValue>("One", "1 as a Value.", new IntValue(1)));

            #endregion

            #region Create operators and operator graph

            var variableCreator = new VariableCreator { Name = "Initialize" };
            var resultsCollector1 = new ResultsCollector();
            var initLayerAnalyzerProcessor = new SubScopesProcessor();
            var layerVariableCreator = new VariableCreator { Name = "Initialize Layer" };
            var initLayerAnalyzerPlaceholder = new Placeholder { Name = "LayerAnalyzer (Placeholder)" };
            var initAnalyzerPlaceholder = new Placeholder { Name = "Analyzer (Placeholder)" };
            var initFinalAnalyzerPlaceholder = new Placeholder {Name = "FinalAnalyzer (Placeholder)"};
            var matingPoolCreator = new MatingPoolCreator { Name = "Create Mating Pools" };
            var matingPoolProcessor = new UniformSubScopesProcessor { Name = "Process Mating Pools" };
            var initializeLayer = new Assigner { Name = "Reset LayerEvaluatedSolutions" };
            var mainOperator = new AlpsNsga2MainOperator();
            var generationsIncrementer = new IntCounter { Name = "Increment Generations" };
            var evaluatedSolutionsReducer = new DataReducer { Name = "Increment EvaluatedSolutions" };
            var eldersEmigrator = CreateEldersEmigrator();
            var layerOpener = CreateLayerOpener();
            var layerReseeder = CreateReseeder();
            var currentPopulationSizeComparator = new Comparator { Name = "Isn't CurrentPopulationSize 0?" };
            var currentPopulationSizeIsNotZeroBranch = new ConditionalBranch { Name = "CurrentPopulationSize != Zero" };
            var layerAnalyzerProcessor = new UniformSubScopesProcessor();
            var layerAnalyzerPlaceholder = new Placeholder { Name = "LayerAnalyzer (Placeholder)" };
            var analyzerPlaceholder = new Placeholder { Name = "Analyzer (Placeholder)" };
            var termination = new TerminationOperator();
            var rankAndCrowdingSorter = new RankAndCrowdingSorter();
            var mergingReducer = new MergingReducer();
            var leftSelector = new LeftSelector();
            var rightReducer = new RightReducer();

            OperatorGraph.InitialOperator = variableCreator;
            
            variableCreator.CollectedValues.Add(new ValueParameter<IntValue>("Generations", new IntValue(0)));
            variableCreator.CollectedValues.Add(new ValueParameter<IntValue>("OpenLayers", new IntValue(1)));
            variableCreator.Successor = initLayerAnalyzerProcessor;

            initLayerAnalyzerProcessor.Operators.Add(layerVariableCreator);
            initLayerAnalyzerProcessor.Successor = initAnalyzerPlaceholder;

            layerVariableCreator.CollectedValues.Add(new ValueParameter<IntValue>("Layer", new IntValue(0)));
            layerVariableCreator.CollectedValues.Add(new ValueParameter<ResultCollection>("LayerResults"));
            layerVariableCreator.Successor = initLayerAnalyzerPlaceholder;

            initLayerAnalyzerPlaceholder.OperatorParameter.ActualName = LayerAnalyzerParameter.Name;
            initLayerAnalyzerPlaceholder.Successor = null;

            initAnalyzerPlaceholder.OperatorParameter.ActualName = AnalyzerParameter.Name;
            initAnalyzerPlaceholder.Successor = resultsCollector1;

            resultsCollector1.CollectedValues.Add(new LookupParameter<IntValue>("Generations"));
            resultsCollector1.CollectedValues.Add(new ScopeTreeLookupParameter<ResultCollection>("LayerResults", "Result set for each Layer", "LayerResults"));
            resultsCollector1.CollectedValues.Add(new LookupParameter<IntValue>("OpenLayers"));
            resultsCollector1.CopyValue = new BoolValue(false);
            resultsCollector1.ResultsParameter.ActualName = ResultsParameter.Name;
            resultsCollector1.Successor = matingPoolCreator;

            matingPoolCreator.MatingPoolRangeParameter.Value = null;
            matingPoolCreator.MatingPoolRangeParameter.ActualName = MatingPoolRangeParameter.Name;
            matingPoolCreator.Successor = matingPoolProcessor;

            matingPoolProcessor.Parallel.Value = true;
            matingPoolProcessor.Operator = initializeLayer;
            matingPoolProcessor.Successor = generationsIncrementer;

            initializeLayer.LeftSideParameter.ActualName = "LayerEvaluatedSolutions";
            initializeLayer.RightSideParameter.Value = new IntValue(0);
            initializeLayer.Successor = mainOperator;

            mainOperator.RandomParameter.ActualName = LocalRandomParameter.Name;
            mainOperator.EvaluatorParameter.ActualName = EvaluatorParameter.Name;
            mainOperator.EvaluatedSolutionsParameter.ActualName = "LayerEvaluatedSolutions";
            mainOperator.QualityParameter.ActualName = QualitiesParameter.Name;
            mainOperator.MaximizationParameter.ActualName = MaximizationParameter.Name;
            mainOperator.PopulationSizeParameter.ActualName = PopulationSizeParameter.Name;
            mainOperator.SelectorParameter.ActualName = SelectorParameter.Name;
            mainOperator.CrossoverParameter.ActualName = CrossoverParameter.Name;
            mainOperator.CrossoverProbabilityParameter.ActualName = CrossoverProbabilityParameter.Name;
            mainOperator.MutatorParameter.ActualName = MutatorParameter.ActualName;
            mainOperator.MutationProbabilityParameter.ActualName = MutationProbabilityParameter.Name;
            mainOperator.AgeParameter.ActualName = AgeParameter.Name;
            mainOperator.AgeInheritanceParameter.ActualName = AgeInheritanceParameter.Name;
            mainOperator.AgeIncrementParameter.Value = new DoubleValue(1.0);
            mainOperator.Successor = null;

            generationsIncrementer.ValueParameter.ActualName = "Generations";
            generationsIncrementer.Increment = new IntValue(1);
            generationsIncrementer.Successor = evaluatedSolutionsReducer;

            evaluatedSolutionsReducer.ParameterToReduce.ActualName = "LayerEvaluatedSolutions";
            evaluatedSolutionsReducer.TargetParameter.ActualName = EvaluatedSolutionsParameter.Name;
            evaluatedSolutionsReducer.ReductionOperation.Value = new ReductionOperation(ReductionOperations.Sum);
            evaluatedSolutionsReducer.TargetOperation.Value = new ReductionOperation(ReductionOperations.Sum);
            evaluatedSolutionsReducer.Successor = eldersEmigrator;

            eldersEmigrator.Successor = layerOpener;

            layerOpener.Successor = layerReseeder;

            layerReseeder.Successor = layerAnalyzerProcessor;

            // Layer analyzer is only performed if individuals count is not 0
            layerAnalyzerProcessor.Operator = currentPopulationSizeComparator;
            layerAnalyzerProcessor.Successor = analyzerPlaceholder;

            currentPopulationSizeComparator.LeftSideParameter.ActualName = CurrentPopulationSizeParameter.Name;
            currentPopulationSizeComparator.RightSideParameter.ActualName = ZeroParameter.Name;
            currentPopulationSizeComparator.ResultParameter.ActualName = "CurrentPopulationSizeIsNotZero";
            currentPopulationSizeComparator.Comparison = new Comparison(ComparisonType.NotEqual);
            currentPopulationSizeComparator.Successor = currentPopulationSizeIsNotZeroBranch;

            currentPopulationSizeIsNotZeroBranch.ConditionParameter.ActualName = "CurrentPopulationSizeIsNotZero";
            currentPopulationSizeIsNotZeroBranch.TrueBranch = layerAnalyzerPlaceholder;

            layerAnalyzerPlaceholder.OperatorParameter.ActualName = LayerAnalyzerParameter.Name;

            analyzerPlaceholder.OperatorParameter.ActualName = AnalyzerParameter.Name;
            analyzerPlaceholder.Successor = termination;

            termination.TerminatorParameter.ActualName = TerminatorParameter.Name;
            termination.ContinueBranch = matingPoolCreator;

            termination.TerminateBranch = mergingReducer;
            mergingReducer.Successor = rankAndCrowdingSorter;

            rankAndCrowdingSorter.DominateOnEqualQualitiesParameter.ActualName = DominateOnEqualQualitiesParameter.Name;
            rankAndCrowdingSorter.CrowdingDistanceParameter.ActualName = "CrowdingDistance";
            rankAndCrowdingSorter.RankParameter.ActualName = "Rank";
            rankAndCrowdingSorter.Successor = leftSelector;

            leftSelector.CopySelected = new BoolValue(false);
            leftSelector.NumberOfSelectedSubScopesParameter.ActualName = PopulationSizeParameter.Name;
            leftSelector.Successor = rightReducer;

            rightReducer.Successor = initFinalAnalyzerPlaceholder;

            initFinalAnalyzerPlaceholder.OperatorParameter.ActualName = FinalAnalyzerParameter.Name;

            #endregion
        }

        private CombinedOperator CreateEldersEmigrator()
        {
            var eldersEmigrator = new CombinedOperator { Name = "Emigrate Elders" };
            var selectorProcessor = new UniformSubScopesProcessor();
            var eldersSelector = new EldersSelector();
            var shiftToRightMigrator = new UnidirectionalRingMigrator { Name = "Shift elders to next layer" };
            var mergingProcessor = new UniformSubScopesProcessor();
            var mergingReducer = new MergingReducer();
            var subScopesCounter1 = new SubScopesCounter();
            var currentPopulationSizeComparator = new Comparator { Name = "Is CurrentPopulationSize greater than 1?" };
            var currentPopulationSizeIsGreaterThanOne = new ConditionalBranch { Name = "CurrentPopulationSize > 1" };
            var reduceToPopulationSizeBranch = new ConditionalBranch { Name = "ReduceToPopulationSize?" };
            var countCalculator = new ExpressionCalculator { Name = "CurrentPopulationSize = Min(CurrentPopulationSize, PopulationSize)" };
            var leftSelector = new LeftSelector();
            var rankAndCrowdingSorter = new RankAndCrowdingSorter();
            var subScopesCounter2 = new SubScopesCounter();
            var rightReducer = new RightReducer();
            
            eldersEmigrator.OperatorGraph.InitialOperator = selectorProcessor;

            selectorProcessor.Operator = eldersSelector;
            selectorProcessor.Successor = shiftToRightMigrator;

            eldersSelector.AgeParameter.ActualName = AgeParameter.Name;
            eldersSelector.AgeLimitsParameter.ActualName = AgeLimitsParameter.Name;
            eldersSelector.NumberOfLayersParameter.ActualName = NumberOfLayersParameter.Name;
            eldersSelector.LayerParameter.ActualName = "Layer";
            eldersSelector.Successor = null;

            shiftToRightMigrator.ClockwiseMigrationParameter.Value = new BoolValue(true);
            shiftToRightMigrator.Successor = mergingProcessor;

            mergingProcessor.Operator = mergingReducer;

            mergingReducer.Successor = subScopesCounter1;

            subScopesCounter1.ValueParameter.ActualName = CurrentPopulationSizeParameter.Name;
            subScopesCounter1.AccumulateParameter.Value = new BoolValue(false);
            subScopesCounter1.Successor = currentPopulationSizeComparator;

            currentPopulationSizeComparator.LeftSideParameter.ActualName = CurrentPopulationSizeParameter.Name;
            currentPopulationSizeComparator.RightSideParameter.ActualName = OneParameter.Name;
            currentPopulationSizeComparator.ResultParameter.ActualName = "CurrentPopulationSizeIsGreaterThanOne";
            currentPopulationSizeComparator.Comparison = new Comparison(ComparisonType.Greater);
            currentPopulationSizeComparator.Successor = currentPopulationSizeIsGreaterThanOne;

            currentPopulationSizeIsGreaterThanOne.ConditionParameter.ActualName = "CurrentPopulationSizeIsGreaterThanOne";
            currentPopulationSizeIsGreaterThanOne.TrueBranch = rankAndCrowdingSorter;
            
            // We have to sort individuals before reducing, because if we shifted some of them to another layer, it can happen that they are not correctly sorted
            rankAndCrowdingSorter.DominateOnEqualQualitiesParameter.ActualName = DominateOnEqualQualitiesParameter.Name;
            rankAndCrowdingSorter.CrowdingDistanceParameter.ActualName = "CrowdingDistance";
            rankAndCrowdingSorter.RankParameter.ActualName = "Rank";
            rankAndCrowdingSorter.Successor = reduceToPopulationSizeBranch;

            reduceToPopulationSizeBranch.ConditionParameter.ActualName = ReduceToPopulationSizeParameter.Name;
            reduceToPopulationSizeBranch.TrueBranch = countCalculator;

            countCalculator.CollectedValues.Add(new LookupParameter<IntValue>(PopulationSizeParameter.Name));
            countCalculator.CollectedValues.Add(new LookupParameter<IntValue>(CurrentPopulationSizeParameter.Name));
            countCalculator.ExpressionParameter.Value = new StringValue("CurrentPopulationSize PopulationSize CurrentPopulationSize PopulationSize < if toint");
            countCalculator.ExpressionResultParameter.ActualName = CurrentPopulationSizeParameter.Name;
            countCalculator.Successor = leftSelector;
            
            leftSelector.CopySelected = new BoolValue(false);
            leftSelector.NumberOfSelectedSubScopesParameter.ActualName = CurrentPopulationSizeParameter.Name;
            leftSelector.Successor = rightReducer;

            rightReducer.Successor = subScopesCounter2;
            subScopesCounter2.ValueParameter.ActualName = CurrentPopulationSizeParameter.Name;
            subScopesCounter2.AccumulateParameter.Value = new BoolValue(false);

            return eldersEmigrator;
        }

        private CombinedOperator CreateLayerOpener()
        {
            var layerOpener = new CombinedOperator { Name = "Open new Layer if needed" };
            var maxLayerReached = new Comparator { Name = "MaxLayersReached = OpenLayers >= NumberOfLayers" };
            var maxLayerReachedBranch = new ConditionalBranch { Name = "MaxLayersReached?" };
            var openNewLayerCalculator = new ExpressionCalculator { Name = "OpenNewLayer = Generations >= AgeLimits[OpenLayers - 1]" };
            var openNewLayerBranch = new ConditionalBranch { Name = "OpenNewLayer?" };
            var layerCreator = new LastLayerCloner { Name = "Create Layer" };
            var updateLayerNumber = new Assigner { Name = "Layer = OpenLayers" };
            var historyWiper = new ResultsHistoryWiper { Name = "Clear History in Results" };
            var createChildrenViaCrossover = new AlpsNsga2MainOperator();
            var incrEvaluatedSolutionsForNewLayer = new SubScopesCounter { Name = "Update EvaluatedSolutions" };
            var incrOpenLayers = new IntCounter { Name = "Incr. OpenLayers" };
            var newLayerResultsCollector = new ResultsCollector { Name = "Collect new Layer Results" };

            layerOpener.OperatorGraph.InitialOperator = maxLayerReached;

            maxLayerReached.LeftSideParameter.ActualName = "OpenLayers";
            maxLayerReached.RightSideParameter.ActualName = NumberOfLayersParameter.Name;
            maxLayerReached.ResultParameter.ActualName = "MaxLayerReached";
            maxLayerReached.Comparison = new Comparison(ComparisonType.GreaterOrEqual);
            maxLayerReached.Successor = maxLayerReachedBranch;

            maxLayerReachedBranch.ConditionParameter.ActualName = "MaxLayerReached";
            maxLayerReachedBranch.FalseBranch = openNewLayerCalculator;

            openNewLayerCalculator.CollectedValues.Add(new LookupParameter<IntArray>(AgeLimitsParameter.Name));
            openNewLayerCalculator.CollectedValues.Add(new LookupParameter<IntValue>("Generations"));
            openNewLayerCalculator.CollectedValues.Add(new LookupParameter<IntValue>(NumberOfLayersParameter.Name));
            openNewLayerCalculator.CollectedValues.Add(new LookupParameter<IntValue>("OpenLayers"));
            openNewLayerCalculator.ExpressionResultParameter.ActualName = "OpenNewLayer";
            openNewLayerCalculator.ExpressionParameter.Value = new StringValue("Generations 1 + AgeLimits OpenLayers 1 - [] >");
            openNewLayerCalculator.Successor = openNewLayerBranch;

            openNewLayerBranch.ConditionParameter.ActualName = "OpenNewLayer";
            openNewLayerBranch.TrueBranch = layerCreator;

            layerCreator.NewLayerOperator = updateLayerNumber;
            layerCreator.Successor = incrOpenLayers;

            updateLayerNumber.LeftSideParameter.ActualName = "Layer";
            updateLayerNumber.RightSideParameter.ActualName = "OpenLayers";
            updateLayerNumber.Successor = historyWiper;

            historyWiper.ResultsParameter.ActualName = "LayerResults";
            historyWiper.Successor = createChildrenViaCrossover;

            createChildrenViaCrossover.RandomParameter.ActualName = LocalRandomParameter.Name;
            createChildrenViaCrossover.EvaluatorParameter.ActualName = EvaluatorParameter.Name;
            createChildrenViaCrossover.EvaluatedSolutionsParameter.ActualName = "LayerEvaluatedSolutions";
            createChildrenViaCrossover.QualityParameter.ActualName = QualitiesParameter.Name;
            createChildrenViaCrossover.MaximizationParameter.ActualName = MaximizationParameter.Name;
            createChildrenViaCrossover.PopulationSizeParameter.ActualName = PopulationSizeParameter.Name;
            createChildrenViaCrossover.SelectorParameter.ActualName = SelectorParameter.Name;
            createChildrenViaCrossover.CrossoverParameter.ActualName = CrossoverParameter.Name;
            createChildrenViaCrossover.CrossoverProbabilityParameter.ActualName = CrossoverProbabilityParameter.Name;
            createChildrenViaCrossover.MutatorParameter.ActualName = MutatorParameter.Name;
            createChildrenViaCrossover.MutationProbabilityParameter.ActualName = MutationProbabilityParameter.Name;
            createChildrenViaCrossover.AgeParameter.ActualName = AgeParameter.Name;
            createChildrenViaCrossover.AgeInheritanceParameter.ActualName = AgeInheritanceParameter.Name;
            createChildrenViaCrossover.AgeIncrementParameter.Value = new DoubleValue(0.0);
            createChildrenViaCrossover.Successor = incrEvaluatedSolutionsForNewLayer;

            incrEvaluatedSolutionsForNewLayer.ValueParameter.ActualName = EvaluatedSolutionsParameter.Name;
            incrEvaluatedSolutionsForNewLayer.AccumulateParameter.Value = new BoolValue(true);

            incrOpenLayers.ValueParameter.ActualName = "OpenLayers";
            incrOpenLayers.Increment = new IntValue(1);
            incrOpenLayers.Successor = newLayerResultsCollector;

            newLayerResultsCollector.CollectedValues.Add(new ScopeTreeLookupParameter<ResultCollection>("LayerResults", "Result set for each layer", "LayerResults"));
            newLayerResultsCollector.CopyValue = new BoolValue(false);
            newLayerResultsCollector.Successor = null;

            return layerOpener;
        }

        private CombinedOperator CreateReseeder()
        {
            var reseeder = new CombinedOperator { Name = "Reseed Layer Zero if needed" };
            var reseedingController = new ReseedingController { Name = "Reseeding needed (Generation % AgeGap == 0)?" };
            var removeIndividuals = new SubScopesRemover();
            var createIndividuals = new SolutionsCreator();
            var initializeAgeProcessor = new UniformSubScopesProcessor();
            var initializeAge = new VariableCreator { Name = "Initialize Age" };
            var incrEvaluatedSolutionsAfterReseeding = new SubScopesCounter { Name = "Update EvaluatedSolutions" };
            var rankAndCrowdingSorter = new RankAndCrowdingSorter();

            reseeder.OperatorGraph.InitialOperator = reseedingController;

            reseedingController.GenerationsParameter.ActualName = "Generations";
            reseedingController.AgeGapParameter.ActualName = AgeGapParameter.Name;
            reseedingController.FirstLayerOperator = removeIndividuals;
            reseedingController.Successor = null;

            removeIndividuals.Successor = createIndividuals;

            createIndividuals.NumberOfSolutionsParameter.ActualName = PopulationSizeParameter.Name;
            createIndividuals.Successor = initializeAgeProcessor;

            initializeAgeProcessor.Operator = initializeAge;
            initializeAgeProcessor.Successor = incrEvaluatedSolutionsAfterReseeding;

            initializeAge.CollectedValues.Add(new ValueParameter<DoubleValue>(AgeParameter.Name, new DoubleValue(0)));

            incrEvaluatedSolutionsAfterReseeding.ValueParameter.ActualName = EvaluatedSolutionsParameter.Name;
            incrEvaluatedSolutionsAfterReseeding.AccumulateParameter.Value = new BoolValue(true);

            incrEvaluatedSolutionsAfterReseeding.Successor = rankAndCrowdingSorter;

            return reseeder;
        }
    }
}