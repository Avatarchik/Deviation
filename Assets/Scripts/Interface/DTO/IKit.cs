﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Interface.DTO
{
	public interface IKit
	{

		string Name { get; set; }
		int ModuleCount { get; set; }
		int MaxModules { get; set; }

		IModule GetCurrentModule();
		IModule GetLeftModule();
		IModule GetRightModule();
		void CycleModuleLeft();
		void CycleModuleRight();
	}
}