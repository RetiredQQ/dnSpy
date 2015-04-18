﻿// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using System.Windows.Threading;
using ICSharpCode.Decompiler;
using dnlib.DotNet;

namespace ICSharpCode.ILSpy.TreeNodes
{
	/// <summary>
	/// Lists the embedded resources in an assembly.
	/// </summary>
	sealed class ResourceListTreeNode : ILSpyTreeNode
	{
		readonly ModuleDef module;
		
		public ResourceListTreeNode(ModuleDef module)
		{
			this.LazyLoading = true;
			this.module = module;
		}
		
		public override object Text {
			get { return ToString(Language); }
		}

		public override string ToString(Language language)
		{
			return "Resources";
		}
		
		public override object Icon {
			get { return Images.FolderClosed; }
		}

		public override object ExpandedIcon {
			get { return Images.FolderOpen; }
		}
		
		protected override void LoadChildren()
		{
			foreach (Resource r in module.Resources.OrderBy(m => m.Name))
				this.Children.Add(ResourceTreeNode.Create(r));
		}
		
		public override FilterResult Filter(FilterSettings settings)
		{
			var visibleFlags = VisibleMembersFlags.Resources;
			if ((settings.Flags & visibleFlags) == 0)
				return FilterResult.Hidden;
			if ((settings.Flags & VisibleMembersFlags.Resources) != 0)
				return FilterResult.Match;	// Make sure it's not hidden
			else if (string.IsNullOrEmpty(settings.SearchTerm))
				return FilterResult.MatchAndRecurse;
			else
				return FilterResult.Recurse;
		}
		
		public override void Decompile(Language language, ITextOutput output, DecompilationOptions options)
		{
			App.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(EnsureLazyChildren));
			foreach (ILSpyTreeNode child in this.Children) {
				child.Decompile(language, output, options);
				output.WriteLine();
			}
		}

		public override NodePathName NodePathName {
			get { return new NodePathName("res"); }
		}
	}
}
