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
using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.Algorithms.AlpsNsga2
{
    [Plugin("HeuristicLab.Algorithms.AlpsNsga2", "Provides an implementation of NSGA-II with ALPS", "3.3.9.0")]
    [PluginFile("HeuristicLab.Algorithms.AlpsNsga2.dll", PluginFileType.Assembly)]
    [PluginDependency("HeuristicLab.Algorithms.ALPS", "3.3")]
    [PluginDependency("HeuristicLab.Analysis", "3.3")]
    [PluginDependency("HeuristicLab.Collections", "3.3")]
    [PluginDependency("HeuristicLab.Common", "3.3")]
    [PluginDependency("HeuristicLab.Core", "3.3")]
    [PluginDependency("HeuristicLab.Data", "3.3")]
    [PluginDependency("HeuristicLab.Encodings.IntegerVectorEncoding", "3.3")]
    [PluginDependency("HeuristicLab.Encodings.RealVectorEncoding", "3.3")]
    [PluginDependency("HeuristicLab.Operators", "3.3")]
    [PluginDependency("HeuristicLab.Optimization", "3.3")]
    [PluginDependency("HeuristicLab.Optimization.Operators", "3.3")]
    [PluginDependency("HeuristicLab.Parameters", "3.3")]
    [PluginDependency("HeuristicLab.Persistence", "3.3")]
    [PluginDependency("HeuristicLab.Random", "3.3")]
    [PluginDependency("HeuristicLab.Selection", "3.3")]
    public class Plugin : PluginBase
    {
    }
}
