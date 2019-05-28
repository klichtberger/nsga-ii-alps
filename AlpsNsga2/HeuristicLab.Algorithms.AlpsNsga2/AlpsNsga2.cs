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
using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Algorithms.AlpsNsga2.Analyzers;
using HeuristicLab.Algorithms.ALPS;
using HeuristicLab.Analysis;
using HeuristicLab.Collections;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Optimization.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.PluginInfrastructure;
using HeuristicLab.Random;

namespace HeuristicLab.Algorithms.AlpsNsga2
{
    [Item("NSGA-II with ALPS",
        "An NSGA-II within an age - layered population structure. The Non-dominated Sorting Genetic Algorithm II was introduced in Deb et al. 2002. A Fast and Elitist Multi-Objective Genetic Algorithm: NSGA-II. IEEE Transactions on Evolutionary Computation, 6(2), pp. 182-197.")]
    [Creatable(CreatableAttribute.Categories.PopulationBasedAlgorithms, Priority = 136)]
    [StorableClass]
    public class AlpsNsga2 : HeuristicOptimizationEngineAlgorithm, IStorableContent
    {
        public string Filename { get; set; }

        #region Problem Properties

        public override Type ProblemType => typeof(IMultiObjectiveHeuristicOptimizationProblem);

        public new IMultiObjectiveHeuristicOptimizationProblem Problem
        {
            get => (IMultiObjectiveHeuristicOptimizationProblem)base.Problem;
            set => base.Problem = value;
        }

        #endregion

        #region Parameter Properties

        private IValueParameter<IntValue> SeedParameter => (IValueParameter<IntValue>)Parameters["Seed"];

        private IValueParameter<BoolValue> SetSeedRandomlyParameter => (IValueParameter<BoolValue>)Parameters["SetSeedRandomly"];

        private IValueParameter<MultiAnalyzer> AnalyzerParameter => (IValueParameter<MultiAnalyzer>)Parameters["Analyzer"];

        private IFixedValueParameter<MultiAnalyzer> LayerAnalyzerParameter => (IFixedValueParameter<MultiAnalyzer>)Parameters["LayerAnalyzer"];

        private IFixedValueParameter<MultiAnalyzer> FinalAnalyzerParameter => (IFixedValueParameter<MultiAnalyzer>)Parameters["FinalAnalyzer"];

        private IValueParameter<IntValue> NumberOfLayersParameter => (IValueParameter<IntValue>)Parameters["NumberOfLayers"];

        private IValueParameter<IntValue> PopulationSizeParameter => (IValueParameter<IntValue>)Parameters["PopulationSize"];

        public IConstrainedValueParameter<ISelector> SelectorParameter => (IConstrainedValueParameter<ISelector>)Parameters["Selector"];

        public IConstrainedValueParameter<ICrossover> CrossoverParameter => (IConstrainedValueParameter<ICrossover>)Parameters["Crossover"];

        private ValueParameter<PercentValue> CrossoverProbabilityParameter => (ValueParameter<PercentValue>)Parameters["CrossoverProbability"];

        public IConstrainedValueParameter<IManipulator> MutatorParameter => (IConstrainedValueParameter<IManipulator>)Parameters["Mutator"];

        private IValueParameter<PercentValue> MutationProbabilityParameter => (IValueParameter<PercentValue>)Parameters["MutationProbability"];

        private IValueParameter<EnumValue<AgingScheme>> AgingSchemeParameter => (IValueParameter<EnumValue<AgingScheme>>)Parameters["AgingScheme"];

        private IValueParameter<IntValue> AgeGapParameter => (IValueParameter<IntValue>)Parameters["AgeGap"];

        private IValueParameter<DoubleValue> AgeInheritanceParameter => (IValueParameter<DoubleValue>)Parameters["AgeInheritance"];

        private IValueParameter<IntArray> AgeLimitsParameter => (IValueParameter<IntArray>)Parameters["AgeLimits"];

        private IValueParameter<IntValue> MatingPoolRangeParameter => (IValueParameter<IntValue>)Parameters["MatingPoolRange"];

        private IValueParameter<BoolValue> ReduceToPopulationSizeParameter => (IValueParameter<BoolValue>)Parameters["ReduceToPopulationSize"];

        private IValueParameter<MultiTerminator> TerminatorParameter => (IValueParameter<MultiTerminator>)Parameters["Terminator"];

        private IFixedValueParameter<BoolValue> DominateOnEqualQualitiesParameter => (IFixedValueParameter<BoolValue>)Parameters["DominateOnEqualQualities"];

        private ValueParameter<IntValue> SelectedParentsParameter => (ValueParameter<IntValue>)Parameters["SelectedParents"];

        #endregion

        #region Properties

        public IntValue Seed
        {
            get => SeedParameter.Value;
            set => SeedParameter.Value = value;
        }

        public BoolValue SetSeedRandomly
        {
            get => SetSeedRandomlyParameter.Value;
            set => SetSeedRandomlyParameter.Value = value;
        }

        public MultiAnalyzer Analyzer
        {
            get => AnalyzerParameter.Value;
            set => AnalyzerParameter.Value = value;
        }

        public MultiAnalyzer LayerAnalyzer => LayerAnalyzerParameter.Value;

        public MultiAnalyzer FinalAnalyzer => FinalAnalyzerParameter.Value;

        public IntValue NumberOfLayers
        {
            get => NumberOfLayersParameter.Value;
            set => NumberOfLayersParameter.Value = value;
        }

        public IntValue PopulationSize
        {
            get => PopulationSizeParameter.Value;
            set => PopulationSizeParameter.Value = value;
        }

        public ISelector Selector
        {
            get => SelectorParameter.Value;
            set => SelectorParameter.Value = value;
        }

        public ICrossover Crossover
        {
            get => CrossoverParameter.Value;
            set => CrossoverParameter.Value = value;
        }

        public PercentValue CrossoverProbability
        {
            get => CrossoverProbabilityParameter.Value;
            set => CrossoverProbabilityParameter.Value = value;
        }

        public IManipulator Mutator
        {
            get => MutatorParameter.Value;
            set => MutatorParameter.Value = value;
        }

        public PercentValue MutationProbability
        {
            get => MutationProbabilityParameter.Value;
            set => MutationProbabilityParameter.Value = value;
        }

        public EnumValue<AgingScheme> AgingScheme
        {
            get => AgingSchemeParameter.Value;
            set => AgingSchemeParameter.Value = value;
        }

        public IntValue AgeGap
        {
            get => AgeGapParameter.Value;
            set => AgeGapParameter.Value = value;
        }

        public DoubleValue AgeInheritance
        {
            get => AgeInheritanceParameter.Value;
            set => AgeInheritanceParameter.Value = value;
        }

        public IntArray AgeLimits
        {
            get => AgeLimitsParameter.Value;
            set => AgeLimitsParameter.Value = value;
        }

        public IntValue MatingPoolRange
        {
            get => MatingPoolRangeParameter.Value;
            set => MatingPoolRangeParameter.Value = value;
        }

        public MultiTerminator Terminators => TerminatorParameter.Value;

        public int MaximumGenerations
        {
            get => generationsTerminator.Threshold.Value;
            set => generationsTerminator.Threshold.Value = value;
        }

        public bool DominateOnEqualQualities
        {
            get => DominateOnEqualQualitiesParameter.Value.Value;
            set => DominateOnEqualQualitiesParameter.Value.Value = value;
        }

        public IntValue SelectedParents
        {
            get => SelectedParentsParameter.Value;
            set => SelectedParentsParameter.Value = value;
        }

        #endregion

        #region Preconfigured Analyzers

        [Storable]
        private OldestAverageYoungestAgeAnalyzer ageAnalyzer;
        [Storable]
        private OldestAverageYoungestAgeAnalyzer layerAgeAnalyzer;
        [Storable]
        private AgeDistributionAnalyzer ageDistributionAnalyzer;
        [Storable]
        private AlpsScatterPlotAnalyzer _paretoFrontAlpsScatterPlot;
        [Storable]
        private AgeDistributionAnalyzer layerAgeDistributionAnalyzer;
        [Storable]
        private RankBasedParetoFrontAnalyzer layerParetoFrontAnalyzer;
        [Storable]
        private ParetoFrontAlpsAnalyzer layerParetoFrontAlpsAnalyzer;
        [Storable]
        private RankBasedParetoFrontAnalyzer finalParetoFrontAnalyzer;
        [Storable]
        private ParetoFrontAlpsAnalyzer finalParetoFrontAlpsAnalyzer;

        #endregion

        #region Preconfigured Terminators

        [Storable]
        private ComparisonTerminator<IntValue> generationsTerminator;
        [Storable]
        private ComparisonTerminator<IntValue> evaluationsTerminator;
        [Storable]
        private ExecutionTimeTerminator executionTimeTerminator;

        #endregion

        #region Helper Properties

        private SolutionsCreator SolutionsCreator => OperatorGraph.Iterate().OfType<SolutionsCreator>().First();

        private UniformSubScopesProcessor UniformSubScopesProcessor => (UniformSubScopesProcessor)SolutionsCreator.Successor;

        private RankAndCrowdingSorter RankAndCrowdingSorter => (RankAndCrowdingSorter)((SubScopesCounter)UniformSubScopesProcessor.Successor).Successor;

        private AlpsNsga2MainLoop MainLoop => OperatorGraph.Iterate().OfType<AlpsNsga2MainLoop>().First();

        #endregion

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new AlpsNsga2(this, cloner);
        }

        [StorableHook(HookType.AfterDeserialization)]
        private void AfterDeserialization()
        {
            // BackwardsCompatibility3.3
            #region Backwards compatible code, remove with 3.4

            if (!Parameters.ContainsKey("DominateOnEqualQualities"))
                Parameters.Add(new FixedValueParameter<BoolValue>("DominateOnEqualQualities", "Flag which determines whether solutions with equal quality values should be treated as dominated.", new BoolValue(false)));
            if (MutatorParameter is OptionalConstrainedValueParameter<IManipulator> optionalMutatorParameter)
            {
                Parameters.Remove(optionalMutatorParameter);
                Parameters.Add(new ConstrainedValueParameter<IManipulator>("Mutator", "The operator used to mutate solutions."));
                foreach (var m in optionalMutatorParameter.ValidValues)
                    MutatorParameter.ValidValues.Add(m);
                if (optionalMutatorParameter.Value == null) MutationProbability.Value = 0; // to guarantee that the old configuration results in the same behavior
                else Mutator = optionalMutatorParameter.Value;
                optionalMutatorParameter.ValidValues.Clear(); // to avoid dangling references to the old parameter its valid values are cleared
            }

            #endregion

            Initialize();
        }

        #region Constructors

        [StorableConstructor]
        private AlpsNsga2(bool deserializing) : base(deserializing) { }

        private AlpsNsga2(AlpsNsga2 original, Cloner cloner) : base(original, cloner)
        {
            layerAgeAnalyzer = cloner.Clone(original.layerAgeAnalyzer);
            layerAgeDistributionAnalyzer = cloner.Clone(original.layerAgeDistributionAnalyzer);
            layerParetoFrontAnalyzer = cloner.Clone(original.layerParetoFrontAnalyzer);
            layerParetoFrontAlpsAnalyzer = cloner.Clone(original.layerParetoFrontAlpsAnalyzer);

            ageAnalyzer = cloner.Clone(original.ageAnalyzer);
            ageDistributionAnalyzer = cloner.Clone(original.ageDistributionAnalyzer);
            _paretoFrontAlpsScatterPlot = cloner.Clone(original._paretoFrontAlpsScatterPlot);
            
            finalParetoFrontAnalyzer = cloner.Clone(original.finalParetoFrontAnalyzer);
            finalParetoFrontAlpsAnalyzer = cloner.Clone(original.finalParetoFrontAlpsAnalyzer);

            generationsTerminator = cloner.Clone(original.generationsTerminator);
            evaluationsTerminator = cloner.Clone(original.evaluationsTerminator);
            executionTimeTerminator = cloner.Clone(original.executionTimeTerminator);

            Initialize();
        }

        public AlpsNsga2() : base()
        {
            #region Add parameters

            Parameters.Add(new ValueParameter<IntValue>("Seed", "The random seed used to initialize the new pseudo random number generator.", new IntValue(0)));
            Parameters.Add(new ValueParameter<BoolValue>("SetSeedRandomly", "True if the random seed should be set to a random value, otherwise false.", new BoolValue(true)));

            Parameters.Add(new FixedValueParameter<MultiAnalyzer>("Analyzer", "The operator used to analyze all individuals from all layers combined.", new MultiAnalyzer()));
            Parameters.Add(new FixedValueParameter<MultiAnalyzer>("LayerAnalyzer", "The operator used to analyze each layer.", new MultiAnalyzer()));
            Parameters.Add(new FixedValueParameter<MultiAnalyzer>("FinalAnalyzer", "The operator used to analyze after termination of the algorithm.", new MultiAnalyzer()));

            Parameters.Add(new ValueParameter<IntValue>("NumberOfLayers", "The number of layers.", new IntValue(10)));
            Parameters.Add(new ValueParameter<IntValue>("PopulationSize", "The size of the population of solutions in each layer.", new IntValue(100)));

            Parameters.Add(new ConstrainedValueParameter<ISelector>("Selector", "The operator used to select solutions for reproduction."));
            Parameters.Add(new ConstrainedValueParameter<ICrossover>("Crossover", "The operator used to cross solutions."));
            Parameters.Add(new ValueParameter<PercentValue>("CrossoverProbability", "The probability that the crossover operator is applied on two parents.", new PercentValue(0.9)));
            Parameters.Add(new ConstrainedValueParameter<IManipulator>("Mutator", "The operator used to mutate solutions."));
            Parameters.Add(new ValueParameter<PercentValue>("MutationProbability", "The probability that the mutation operator is applied on a solution.", new PercentValue(0.05)));

            Parameters.Add(new ValueParameter<EnumValue<AgingScheme>>("AgingScheme", "The aging scheme for setting the age-limits for the layers.", new EnumValue<AgingScheme>(ALPS.AgingScheme.Polynomial)));
            Parameters.Add(new ValueParameter<IntValue>("AgeGap", "The frequency of reseeding the lowest layer and scaling factor for the age-limits for the layers.", new IntValue(20)));
            Parameters.Add(new ValueParameter<DoubleValue>("AgeInheritance", "A weight that determines the age of a child after crossover based on the older (1.0) and younger (0.0) parent.", new DoubleValue(1.0)) { Hidden = true });
            Parameters.Add(new ValueParameter<IntArray>("AgeLimits", "The maximum age an individual is allowed to reach in a certain layer.", new IntArray(new int[0])) { Hidden = true });

            Parameters.Add(new ValueParameter<IntValue>("MatingPoolRange", "The range of layers used for creating a mating pool. (1 = current + previous layer)", new IntValue(1)) { Hidden = true });
            
            Parameters.Add(new ValueParameter<BoolValue>("ReduceToPopulationSize", "Reduce the CurrentPopulationSize after elder migration to PopulationSize", new BoolValue(true)) { Hidden = true });

            Parameters.Add(new ValueParameter<MultiTerminator>("Terminator", "The termination criteria that defines if the algorithm should continue or stop.", new MultiTerminator()));

            Parameters.Add(new ValueParameter<IntValue>("SelectedParents", "Each two parents form a new child, typically this value should be twice the population size, but because the NSGA-II is maximally elitist it can be any multiple of 2 greater than 0.", new IntValue(200)));
            Parameters.Add(new FixedValueParameter<BoolValue>("DominateOnEqualQualities", "Flag which determines whether solutions with equal quality values should be treated as dominated.", new BoolValue(false)));

            #endregion

            #region Create operators

            var globalRandomCreator = new RandomCreator();
            var layer0Creator = new SubScopesCreator { Name = "Create Layer Zero" };
            var layer0Processor = new SubScopesProcessor();
            var localRandomCreator = new LocalRandomCreator();
            var layerSolutionsCreator = new SolutionsCreator();
            var initializeAgeProcessor = new UniformSubScopesProcessor();
            var initializeAge = new VariableCreator { Name = "Initialize Age" };

            var initializeCurrentPopulationSize = new SubScopesCounter { Name = "Initialize CurrentPopulationCounter" };

            var rankAndCrowdingSorter = new RankAndCrowdingSorter();

            var initializeLocalEvaluatedSolutions = new Assigner { Name = "Initialize LayerEvaluatedSolutions" };
            var initializeGlobalEvaluatedSolutions = new DataReducer { Name = "Initialize EvaluatedSolutions" };
            var resultsCollector = new ResultsCollector();
            var mainLoop = new AlpsNsga2MainLoop();

            #endregion

            #region Create and parameterize operator graph

            OperatorGraph.InitialOperator = globalRandomCreator;

            globalRandomCreator.RandomParameter.ActualName = "GlobalRandom";
            globalRandomCreator.SeedParameter.Value = null;
            globalRandomCreator.SeedParameter.ActualName = SeedParameter.Name;
            globalRandomCreator.SetSeedRandomlyParameter.Value = null;
            globalRandomCreator.SetSeedRandomlyParameter.ActualName = SetSeedRandomlyParameter.Name;
            globalRandomCreator.Successor = layer0Creator;

            layer0Creator.NumberOfSubScopesParameter.Value = new IntValue(1);
            layer0Creator.Successor = layer0Processor;

            layer0Processor.Operators.Add(localRandomCreator);
            layer0Processor.Successor = initializeGlobalEvaluatedSolutions;

            localRandomCreator.Successor = layerSolutionsCreator;

            layerSolutionsCreator.NumberOfSolutionsParameter.ActualName = PopulationSizeParameter.Name;
            layerSolutionsCreator.Successor = initializeAgeProcessor;

            initializeAgeProcessor.Operator = initializeAge;
            initializeAgeProcessor.Successor = initializeCurrentPopulationSize;

            initializeCurrentPopulationSize.ValueParameter.ActualName = "CurrentPopulationSize";

            initializeCurrentPopulationSize.Successor = rankAndCrowdingSorter;
            rankAndCrowdingSorter.DominateOnEqualQualitiesParameter.ActualName = DominateOnEqualQualitiesParameter.Name;
            rankAndCrowdingSorter.CrowdingDistanceParameter.ActualName = "CrowdingDistance";
            rankAndCrowdingSorter.RankParameter.ActualName = "Rank";
            rankAndCrowdingSorter.Successor = initializeLocalEvaluatedSolutions;

            initializeAge.CollectedValues.Add(new ValueParameter<DoubleValue>("Age", new DoubleValue(0)));
            initializeAge.Successor = null;

            initializeLocalEvaluatedSolutions.LeftSideParameter.ActualName = "LayerEvaluatedSolutions";
            initializeLocalEvaluatedSolutions.RightSideParameter.ActualName = "CurrentPopulationSize";
            initializeLocalEvaluatedSolutions.Successor = null;

            initializeGlobalEvaluatedSolutions.ReductionOperation.Value.Value = ReductionOperations.Sum;
            initializeGlobalEvaluatedSolutions.TargetOperation.Value.Value = ReductionOperations.Assign;
            initializeGlobalEvaluatedSolutions.ParameterToReduce.ActualName = "LayerEvaluatedSolutions";
            initializeGlobalEvaluatedSolutions.TargetParameter.ActualName = "EvaluatedSolutions";

            initializeGlobalEvaluatedSolutions.Successor = resultsCollector;

            resultsCollector.CollectedValues.Add(new LookupParameter<IntValue>("Evaluated Solutions", null, "EvaluatedSolutions"));
            resultsCollector.ResultsParameter.ActualName = "Results";

            resultsCollector.Successor = mainLoop;

            mainLoop.GlobalRandomParameter.ActualName = "GlobalRandom";
            mainLoop.LocalRandomParameter.ActualName = localRandomCreator.LocalRandomParameter.Name;
            mainLoop.EvaluatedSolutionsParameter.ActualName = "EvaluatedSolutions";
            mainLoop.AnalyzerParameter.ActualName = AnalyzerParameter.Name;
            mainLoop.LayerAnalyzerParameter.ActualName = LayerAnalyzerParameter.Name;
            mainLoop.FinalAnalyzerParameter.ActualName = FinalAnalyzerParameter.Name;
            mainLoop.NumberOfLayersParameter.ActualName = NumberOfLayersParameter.Name;
            mainLoop.PopulationSizeParameter.ActualName = PopulationSizeParameter.Name;
            mainLoop.CurrentPopulationSizeParameter.ActualName = "CurrentPopulationSize";
            mainLoop.SelectorParameter.ActualName = SelectorParameter.Name;
            mainLoop.CrossoverParameter.ActualName = CrossoverParameter.Name;
            mainLoop.CrossoverProbabilityParameter.ActualName = CrossoverProbabilityParameter.Name;
            mainLoop.MutatorParameter.ActualName = MutatorParameter.Name;
            mainLoop.MutationProbabilityParameter.ActualName = MutationProbabilityParameter.Name;
            mainLoop.AgeParameter.ActualName = "Age";
            mainLoop.AgeGapParameter.ActualName = AgeGapParameter.Name;
            mainLoop.AgeInheritanceParameter.ActualName = AgeInheritanceParameter.Name;
            mainLoop.AgeLimitsParameter.ActualName = AgeLimitsParameter.Name;
            mainLoop.MatingPoolRangeParameter.ActualName = MatingPoolRangeParameter.Name;
            mainLoop.ReduceToPopulationSizeParameter.ActualName = ReduceToPopulationSizeParameter.Name;
            mainLoop.TerminatorParameter.ActualName = TerminatorParameter.Name;

            #endregion

            #region Set selectors

            foreach (var selector in ApplicationManager.Manager.GetInstances<ISelector>().Where(s => !(s is ISingleObjectiveSelector)).OrderBy(s => Name))
                SelectorParameter.ValidValues.Add(selector);
            var tournamentSelector = SelectorParameter.ValidValues.FirstOrDefault(x => x.GetType().Name.Equals("CrowdedTournamentSelector"));
            if (tournamentSelector != null) SelectorParameter.Value = tournamentSelector;

            #endregion

            #region Create analyzers

            ageAnalyzer = new OldestAverageYoungestAgeAnalyzer();
            layerAgeAnalyzer = new OldestAverageYoungestAgeAnalyzer();
            ageDistributionAnalyzer = new AgeDistributionAnalyzer();
            layerAgeDistributionAnalyzer = new AgeDistributionAnalyzer();
            layerParetoFrontAnalyzer = new RankBasedParetoFrontAnalyzer();
            finalParetoFrontAnalyzer = new RankBasedParetoFrontAnalyzer();
            finalParetoFrontAlpsAnalyzer = new ParetoFrontAlpsAnalyzer();
            _paretoFrontAlpsScatterPlot = new AlpsScatterPlotAnalyzer();
            layerParetoFrontAlpsAnalyzer = new ParetoFrontAlpsAnalyzer();

            #endregion

            #region Create terminators

            generationsTerminator = new ComparisonTerminator<IntValue>("Generations", ComparisonType.Less, new IntValue(1000)) { Name = "Generations" };
            evaluationsTerminator = new ComparisonTerminator<IntValue>("EvaluatedSolutions", ComparisonType.Less, new IntValue(int.MaxValue)) { Name = "Evaluations" };
            executionTimeTerminator = new ExecutionTimeTerminator(this, new TimeSpanValue(TimeSpan.FromMinutes(5)));

            #endregion

            #region Parameterize

            _paretoFrontAlpsScatterPlot = new AlpsScatterPlotAnalyzer();
            _paretoFrontAlpsScatterPlot.RankParameter.ActualName = "Rank";
            _paretoFrontAlpsScatterPlot.RankParameter.Depth = 1;
            _paretoFrontAlpsScatterPlot.ResultsParameter.ActualName = "Results";
            
            layerParetoFrontAnalyzer = new RankBasedParetoFrontAnalyzer();
            layerParetoFrontAnalyzer.RankParameter.ActualName = "Rank";
            layerParetoFrontAnalyzer.RankParameter.Depth = 1;
            layerParetoFrontAnalyzer.ResultsParameter.ActualName = "Results";

            layerParetoFrontAlpsAnalyzer = new ParetoFrontAlpsAnalyzer();
            layerParetoFrontAlpsAnalyzer.RankParameter.ActualName = "Rank";
            layerParetoFrontAlpsAnalyzer.RankParameter.Depth = 1;
            layerParetoFrontAlpsAnalyzer.ResultsParameter.ActualName = "Results";
            
            finalParetoFrontAnalyzer = new RankBasedParetoFrontAnalyzer();
            finalParetoFrontAnalyzer.RankParameter.ActualName = "Rank";
            finalParetoFrontAnalyzer.RankParameter.Depth = 1;
            finalParetoFrontAnalyzer.ResultsParameter.ActualName = "Results";

            finalParetoFrontAlpsAnalyzer = new ParetoFrontAlpsAnalyzer();
            finalParetoFrontAlpsAnalyzer.RankParameter.ActualName = "Rank";
            finalParetoFrontAlpsAnalyzer.RankParameter.Depth = 1;
            finalParetoFrontAlpsAnalyzer.ResultsParameter.ActualName = "Results";

            ParametrizeAnalyzers();
            UpdateAnalyzers();
            ParametrizeSelectors();
            UpdateTerminators();
            ParametrizeAgeLimits();

            #endregion

            Initialize();
        }

        #endregion

        public override void Prepare()
        {
            if (Problem != null) base.Prepare();
        }

        #region Events

        protected override void OnProblemChanged()
        {
            base.OnProblemChanged();
            ParametrizeStochasticOperator(Problem.SolutionCreator);
            ParametrizeStochasticOperatorForLayer(Problem.Evaluator);

            foreach (var op in Problem.Operators.OfType<IOperator>())
            {
                ParametrizeStochasticOperator(op);
            }

            UpdateAnalyzers();
            UpdateCrossovers();
            UpdateMutators();
            UpdateTerminators();

            ParametrizeSolutionsCreator();
            ParametrizeRankAndCrowdingSorter();
            ParametrizeMainLoop();
            ParametrizeSelectors();
            ParametrizeAnalyzers();
            ParametrizeIterationBasedOperators();
            UpdateCrossovers();
            UpdateMutators();
            UpdateAnalyzers();
            Problem.Evaluator.QualitiesParameter.ActualNameChanged += Evaluator_QualitiesParameter_ActualNameChanged;

        }

        protected override void Problem_SolutionCreatorChanged(object sender, EventArgs e)
        {
            base.Problem_SolutionCreatorChanged(sender, e);
            ParametrizeStochasticOperator(Problem.SolutionCreator);
            ParametrizeStochasticOperatorForLayer(Problem.Evaluator);

            Problem.Evaluator.QualitiesParameter.ActualNameChanged += Evaluator_QualitiesParameter_ActualNameChanged;

            ParametrizeSolutionsCreator();
            ParametrizeAnalyzers();
        }

        protected override void Problem_EvaluatorChanged(object sender, EventArgs e)
        {
            base.Problem_EvaluatorChanged(sender, e);
            ParametrizeStochasticOperatorForLayer(Problem.Evaluator);

            foreach (var @operator in Problem.Operators.OfType<IOperator>())
            {
                ParametrizeStochasticOperator(@operator);
            }

            UpdateAnalyzers();
            ParametrizeSolutionsCreator();
            ParametrizeMainLoop();
            ParametrizeSelectors();
            ParametrizeRankAndCrowdingSorter();
            ParametrizeAnalyzers();
            Problem.Evaluator.QualitiesParameter.ActualNameChanged += Evaluator_QualitiesParameter_ActualNameChanged;
        }

        protected override void Problem_OperatorsChanged(object sender, EventArgs e)
        {
            foreach (var op in Problem.Operators.OfType<IOperator>()) ParametrizeStochasticOperator(op);
            ParametrizeIterationBasedOperators();
            UpdateCrossovers();
            UpdateMutators();
            UpdateAnalyzers();
            base.Problem_OperatorsChanged(sender, e);
        }

        private void PopulationSizeParameter_ValueChanged(object sender, EventArgs e)
        {
            PopulationSize.ValueChanged += PopulationSize_ValueChanged;
            ParametrizeSelectors();
        }

        private void PopulationSize_ValueChanged(object sender, EventArgs e)
        {
            ParametrizeSelectors();
        }

        private void SelectedParentsParameter_ValueChanged(object sender, EventArgs e)
        {
            SelectedParents.ValueChanged += SelectedParents_ValueChanged;
            SelectedParents_ValueChanged(null, EventArgs.Empty);
        }

        private void SelectedParents_ValueChanged(object sender, EventArgs e)
        {
            if (SelectedParents.Value < 2) SelectedParents.Value = 2;
            else if (SelectedParents.Value % 2 != 0)
            {
                SelectedParents.Value = SelectedParents.Value + 1;
            }
        }

        private void Evaluator_QualitiesParameter_ActualNameChanged(object sender, EventArgs e)
        {
            ParametrizeRankAndCrowdingSorter();
            ParametrizeMainLoop();
            ParametrizeSelectors();
            ParametrizeAnalyzers();
        }

        private void AgeGapParameter_ValueChanged(object sender, EventArgs e)
        {
            AgeGap.ValueChanged += AgeGap_ValueChanged;
            ParametrizeAgeLimits();
        }

        private void AgeGap_ValueChanged(object sender, EventArgs e)
        {
            ParametrizeAgeLimits();
        }

        private void AgingSchemeParameter_ValueChanged(object sender, EventArgs e)
        {
            AgingScheme.ValueChanged += AgingScheme_ValueChanged;
            ParametrizeAgeLimits();
        }

        private void AgingScheme_ValueChanged(object sender, EventArgs e)
        {
            ParametrizeAgeLimits();
        }

        private void NumberOfLayers_ValueChanged(object sender, EventArgs e)
        {
            ParametrizeAgeLimits();
        }

        private void AnalyzerOperators_ItemsAdded(object sender, CollectionItemsChangedEventArgs<IndexedItem<IAnalyzer>> e)
        {
            foreach (var analyzer in e.Items)
            {
                foreach (var parameter in analyzer.Value.Parameters.OfType<IScopeTreeLookupParameter>())
                {
                    parameter.Depth = 2;
                }
            }
        }

        private void LayerAnalyzerOperators_ItemsAdded(object sender, CollectionItemsChangedEventArgs<IndexedItem<IAnalyzer>> e)
        {
            foreach (var analyzer in e.Items)
            {
                if (analyzer.Value.Parameters.TryGetValue("Results", out var resultParameter))
                {
                    if (resultParameter is ILookupParameter lookupParameter)
                        lookupParameter.ActualName = "LayerResults";
                }
                foreach (var parameter in analyzer.Value.Parameters.OfType<IScopeTreeLookupParameter>())
                {
                    parameter.Depth = 1;
                }
            }
        }

        private void FinalAnalyzerOperators_ItemsAdded(object sender, CollectionItemsChangedEventArgs<IndexedItem<IAnalyzer>> e)
        {
            foreach (var analyzer in e.Items)
            {
                foreach (var parameter in analyzer.Value.Parameters.OfType<IScopeTreeLookupParameter>())
                {
                    parameter.Depth = 1;
                }
            }
        }

        #endregion

        #region Helpers

        private void Initialize()
        {
            if (Problem != null)
                Problem.Evaluator.QualitiesParameter.ActualNameChanged += Evaluator_QualitiesParameter_ActualNameChanged;

            NumberOfLayersParameter.Value.ValueChanged += NumberOfLayers_ValueChanged;
            NumberOfLayers.ValueChanged += NumberOfLayers_ValueChanged;

            Analyzer.Operators.ItemsAdded += AnalyzerOperators_ItemsAdded;
            LayerAnalyzer.Operators.ItemsAdded += LayerAnalyzerOperators_ItemsAdded;
            FinalAnalyzer.Operators.ItemsAdded += FinalAnalyzerOperators_ItemsAdded;

            AgeGapParameter.ValueChanged += AgeGapParameter_ValueChanged;
            AgeGap.ValueChanged += AgeGap_ValueChanged;
            AgingSchemeParameter.ValueChanged += AgingSchemeParameter_ValueChanged;
            AgingScheme.ValueChanged += AgingScheme_ValueChanged;

            PopulationSizeParameter.ValueChanged += PopulationSizeParameter_ValueChanged;
            PopulationSize.ValueChanged += PopulationSize_ValueChanged;

            SelectedParentsParameter.ValueChanged += SelectedParentsParameter_ValueChanged;
            SelectedParents.ValueChanged += SelectedParents_ValueChanged;
        }

        private void ParametrizeSolutionsCreator()
        {
            SolutionsCreator.EvaluatorParameter.ActualName = Problem.EvaluatorParameter.Name;
            SolutionsCreator.SolutionCreatorParameter.ActualName = Problem.SolutionCreatorParameter.Name;
        }

        private void ParametrizeRankAndCrowdingSorter()
        {
            RankAndCrowdingSorter.MaximizationParameter.ActualName = Problem.MaximizationParameter.Name;
            RankAndCrowdingSorter.QualitiesParameter.ActualName = Problem.Evaluator.QualitiesParameter.ActualName;
        }

        private void ParametrizeMainLoop()
        {
            MainLoop.EvaluatorParameter.ActualName = Problem.EvaluatorParameter.Name;
            MainLoop.MaximizationParameter.ActualName = Problem.MaximizationParameter.Name;
            MainLoop.QualitiesParameter.ActualName = Problem.Evaluator.QualitiesParameter.ActualName;
        }

        private void ParametrizeStochasticOperator(IOperator op)
        {
            if (op is IStochasticOperator @operator)
                @operator.RandomParameter.ActualName = "GlobalRandom";
        }

        private void ParametrizeSelectors()
        {
            foreach (var selector in SelectorParameter.ValidValues)
            {
                selector.CopySelected = new BoolValue(true);
                selector.NumberOfSelectedSubScopesParameter.ActualName = SelectedParentsParameter.Name;
                ParametrizeStochasticOperator(selector);
            }
            if (Problem != null)
            {
                foreach (var selector in SelectorParameter.ValidValues.OfType<IMultiObjectiveSelector>())
                {
                    selector.MaximizationParameter.ActualName = Problem.MaximizationParameter.Name;
                    selector.QualitiesParameter.ActualName = Problem.Evaluator.QualitiesParameter.ActualName;
                }
            }
        }

        private void ParametrizeAnalyzers()
        {
            if (Problem != null)
            {
                _paretoFrontAlpsScatterPlot.QualitiesParameter.ActualName = Problem.Evaluator.QualitiesParameter.ActualName;
                _paretoFrontAlpsScatterPlot.QualitiesParameter.Depth = 1;

                layerParetoFrontAnalyzer.QualitiesParameter.ActualName = Problem.Evaluator.QualitiesParameter.ActualName;
                layerParetoFrontAnalyzer.QualitiesParameter.Depth = 1;

                layerParetoFrontAlpsAnalyzer.QualitiesParameter.ActualName = Problem.Evaluator.QualitiesParameter.ActualName;
                layerParetoFrontAlpsAnalyzer.QualitiesParameter.Depth = 1;

                finalParetoFrontAnalyzer.QualitiesParameter.ActualName = Problem.Evaluator.QualitiesParameter.ActualName;
                finalParetoFrontAnalyzer.QualitiesParameter.Depth = 1;
                finalParetoFrontAlpsAnalyzer.QualitiesParameter.ActualName = Problem.Evaluator.QualitiesParameter.ActualName;
                finalParetoFrontAlpsAnalyzer.QualitiesParameter.Depth = 1;
            }
        }

        private void ParametrizeIterationBasedOperators()
        {
            if (Problem != null)
            {
                foreach (var @operator in Problem.Operators.OfType<IIterationBasedOperator>())
                {
                    @operator.IterationsParameter.ActualName = "Generations";
                    @operator.IterationsParameter.Hidden = true;
                    @operator.MaximumIterationsParameter.ActualName = generationsTerminator.ThresholdParameter.Name;
                    @operator.MaximumIterationsParameter.Hidden = true;
                }
            }
        }

        private void ParametrizeAgeLimits()
        {
            var scheme = AgingScheme.Value;
            var ageGap = AgeGap.Value;
            var numberOfLayers = NumberOfLayers.Value;
            AgeLimits = scheme.CalculateAgeLimits(ageGap, numberOfLayers);
        }

        private void ParametrizeStochasticOperatorForLayer(IOperator @operator)
        {
            if (@operator is IStochasticOperator stochasticOperator)
            {
                stochasticOperator.RandomParameter.ActualName = "LocalRandom";
                stochasticOperator.RandomParameter.Hidden = true;
            }
        }

        private void UpdateCrossovers()
        {
            var oldCrossover = CrossoverParameter.Value;
            var defaultCrossover = Problem.Operators.OfType<ICrossover>().FirstOrDefault();
            CrossoverParameter.ValidValues.Clear();
            foreach (var crossover in Problem.Operators.OfType<ICrossover>().OrderBy(x => x.Name))
                CrossoverParameter.ValidValues.Add(crossover);
            if (oldCrossover != null)
            {
                var crossover = CrossoverParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldCrossover.GetType());
                if (crossover != null) CrossoverParameter.Value = crossover;
                else oldCrossover = null;
            }
            if (oldCrossover == null && defaultCrossover != null)
                CrossoverParameter.Value = defaultCrossover;
        }

        private void UpdateMutators()
        {
            var oldMutator = MutatorParameter.Value;
            MutatorParameter.ValidValues.Clear();
            var defaultMutator = Problem.Operators.OfType<IManipulator>().FirstOrDefault();
            foreach (var mutator in Problem.Operators.OfType<IManipulator>().OrderBy(x => x.Name))
                MutatorParameter.ValidValues.Add(mutator);
            if (oldMutator != null)
            {
                var mutator = MutatorParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldMutator.GetType());
                if (mutator != null) MutatorParameter.Value = mutator;
                else oldMutator = null;
            }
            if (oldMutator == null && defaultMutator != null)
                MutatorParameter.Value = defaultMutator;
        }

        private void UpdateAnalyzers()
        {
            Analyzer.Operators.Clear();
            LayerAnalyzer.Operators.Clear();
            FinalAnalyzer.Operators.Clear();

            Analyzer.Operators.Add(ageAnalyzer, ageAnalyzer.EnabledByDefault);
            Analyzer.Operators.Add(ageDistributionAnalyzer, ageDistributionAnalyzer.EnabledByDefault);
            Analyzer.Operators.Add(_paretoFrontAlpsScatterPlot, _paretoFrontAlpsScatterPlot.EnabledByDefault);
            
            LayerAnalyzer.Operators.Add(layerAgeAnalyzer, false);
            LayerAnalyzer.Operators.Add(layerAgeDistributionAnalyzer, false);
            LayerAnalyzer.Operators.Add(layerParetoFrontAnalyzer, false);
            LayerAnalyzer.Operators.Add(layerParetoFrontAlpsAnalyzer, false);

            FinalAnalyzer.Operators.Add(finalParetoFrontAlpsAnalyzer, finalParetoFrontAlpsAnalyzer.EnabledByDefault);
            FinalAnalyzer.Operators.Add(finalParetoFrontAnalyzer, finalParetoFrontAnalyzer.EnabledByDefault);

            if (Problem != null)
            {
                foreach (var analyzer in Problem.Operators.OfType<IAnalyzer>())
                {
                    // Analyzer.Operators.Add(analyzer, analyzer.EnabledByDefault);
                    FinalAnalyzer.Operators.Add(analyzer, analyzer.EnabledByDefault);

                    // Exclude this one, because it won't work correctly for layers
                    if (!(analyzer is MultiObjectiveAnalyzer))
                    {
                        LayerAnalyzer.Operators.Add((IAnalyzer)analyzer.Clone(), false);
                    }
                }
            }

        }

        private void UpdateTerminators()
        {
            var newTerminators = new Dictionary<ITerminator, bool> {
                {generationsTerminator, !Terminators.Operators.Contains(generationsTerminator) || Terminators.Operators.ItemChecked(generationsTerminator)},
                {evaluationsTerminator, Terminators.Operators.Contains(evaluationsTerminator) && Terminators.Operators.ItemChecked(evaluationsTerminator)},
                {executionTimeTerminator, Terminators.Operators.Contains(executionTimeTerminator) && Terminators.Operators.ItemChecked(executionTimeTerminator)}
            };
            if (Problem != null)
            {
                foreach (var terminator in Problem.Operators.OfType<ITerminator>())
                    newTerminators.Add(terminator, !Terminators.Operators.Contains(terminator) || Terminators.Operators.ItemChecked(terminator));
            }

            Terminators.Operators.Clear();

            foreach (var newTerminator in newTerminators)
                Terminators.Operators.Add(newTerminator.Key, newTerminator.Value);
        }

        #endregion
    }
}