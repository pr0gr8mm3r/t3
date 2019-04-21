﻿using System;
using System.Collections.Generic;
using System.Numerics;
using T3.Core.Operator;
using T3.Gui;
using Random = System.Random;

namespace T3
{
    class MockModel
    {
        public MockModel()
        {
            Init();
        }

        class AddOperator : Instance<AddOperator>
        {
            [OperatorAttribute(OperatorAttribute.OperatorType.Output)]
            public readonly Slot<float> Result = new Slot<float>();

            public AddOperator()
            {
                Result.UpdateAction = Update;
            }

            private void Update(EvaluationContext context)
            {
                Result.Value = Input1.GetValue(context) + Input2.GetValue(context);
            }

            public readonly InputSlot<float> Input1 = new InputSlot<float>(5.0f);
            public readonly InputSlot<float> Input2 = new InputSlot<float>(10.0f);
        }

        class RandomOperator : Instance<RandomOperator>
        {
            [OperatorAttribute(OperatorAttribute.OperatorType.Output)]
            public readonly Slot<float> Result = new Slot<float>();

            public RandomOperator()
            {
                Result.UpdateAction = Update;
            }

            private void Update(EvaluationContext context)
            {
                var random = new Random(Seed.GetValue(context));
                Result.Value = (float)random.NextDouble();
            }

            public readonly InputSlot<int> Seed = new InputSlot<int>(0);
        }

        class StringLengthOperator : Instance<StringLengthOperator>
        {
            [OperatorAttribute(OperatorAttribute.OperatorType.Output)]
            public readonly Slot<int> Length = new Slot<int>();

            public StringLengthOperator()
            {
                Length.UpdateAction = Update;
            }

            private void Update(EvaluationContext context)
            {
                Length.Value = InputString.GetValue(context).Length;
            }

            public readonly InputSlot<string> InputString = new InputSlot<string>(string.Empty);
        }

        class StringConcatOperator : Instance<StringConcatOperator>
        {
            [OperatorAttribute(OperatorAttribute.OperatorType.Output)]
            public readonly Slot<string> Result = new Slot<string>();

            public StringConcatOperator()
            {
                Result.UpdateAction = Update;
            }

            private void Update(EvaluationContext context)
            {
                Result.Value = Input1.GetValue(context) + Input2.GetValue(context);
            }

            public readonly InputSlot<string> Input1 = new InputSlot<string>(string.Empty);
            public readonly InputSlot<string> Input2 = new InputSlot<string>(string.Empty);
        }

        class ProjectOperator : Instance<ProjectOperator>
        {
        }

        private void Init()
        {
            var addSymbol = new Symbol()
                            {
                                Id = Guid.NewGuid(),
                                SymbolName = "Add",
                                InstanceType = typeof(AddOperator),
                                InputDefinitions =
                                {
                                    new Symbol.InputDefinition { Id = Guid.NewGuid(), Name = "Value1", DefaultValue = new InputValue<float>(5.0f) },
                                    new Symbol.InputDefinition { Id = Guid.NewGuid(), Name = "Value1", DefaultValue = new InputValue<float>(10.0f) }
                                },
                                OutputDefinitions = { new Symbol.OutputDefinition { Id = Guid.NewGuid(), Name = "Result", ValueType = typeof(float) } }
                            };
            var randomSymbol = new Symbol()
                               {
                                   Id = Guid.NewGuid(),
                                   SymbolName = "Random",
                                   InstanceType = typeof(RandomOperator),
                                   InputDefinitions =
                                   {
                                       new Symbol.InputDefinition { Id = Guid.NewGuid(), Name = "Seed", DefaultValue = new InputValue<int>(42) }
                                   },
                                   OutputDefinitions = { new Symbol.OutputDefinition { Id = Guid.NewGuid(), Name = "Random Value", ValueType = typeof(float) } }
                               };
            var stringLengthSymbol = new Symbol()
                                     {
                                         Id = Guid.NewGuid(),
                                         SymbolName = "StringLength",
                                         InstanceType = typeof(StringLengthOperator),
                                         InputDefinitions =
                                         {
                                             new Symbol.InputDefinition() { Id = Guid.NewGuid(), Name = "InputString", DefaultValue = new InputValue<string>(string.Empty) } 
                                         },
                                         OutputDefinitions = { new Symbol.OutputDefinition { Id = Guid.NewGuid(), Name = "Length", ValueType = typeof(int) } }
                                     };
            var stringConcatSymbol = new Symbol()
                                     {
                                         Id = Guid.NewGuid(),
                                         SymbolName = "StringConcat",
                                         InstanceType = typeof(StringConcatOperator),
                                         InputDefinitions =
                                         {
                                             new Symbol.InputDefinition { Id = Guid.NewGuid(), Name = "Input1", DefaultValue = new InputValue<string>(string.Empty) },
                                             new Symbol.InputDefinition { Id = Guid.NewGuid(), Name = "Input2", DefaultValue = new InputValue<string>(string.Empty) }
                                         },
                                         OutputDefinitions = { new Symbol.OutputDefinition { Id = Guid.NewGuid(), Name = "Result", ValueType = typeof(string) } }
                                     };
            var projectSymbol = new Symbol()
                                {
                                    Id = Guid.NewGuid(),
                                    SymbolName = "Project",
                                    InstanceType = typeof(ProjectOperator),
                                    Children =
                                    {
                                        new SymbolChild(addSymbol),
                                        new SymbolChild(addSymbol),
                                        new SymbolChild(randomSymbol),
                                    }
                                };

            projectSymbol.AddConnection(new Symbol.Connection(sourceChildId: projectSymbol.Children[2].Id, // from Random
                                                              outputDefinitionId: randomSymbol.OutputDefinitions[0].Id,
                                                              targetChildId: projectSymbol.Children[0].Id, // to Add
                                                              inputDefinitionId: addSymbol.InputDefinitions[0].Id));

            // register the symbols globally
            var symbols = SymbolRegistry.Entries;
            symbols.Add(addSymbol.Id, addSymbol);
            symbols.Add(randomSymbol.Id, randomSymbol);
            symbols.Add(stringLengthSymbol.Id, stringLengthSymbol);
            symbols.Add(stringConcatSymbol.Id, stringConcatSymbol);
            symbols.Add(projectSymbol.Id, projectSymbol);

            // create instance of project op, all children are create automatically
            Instance projectOp = projectSymbol.CreateInstance();

            // create ui data for project symbol
            var uiEntries = SymbolChildUiRegistry.Entries;
            uiEntries.Add(projectSymbol.Id, new Dictionary<Guid, SymbolChildUi>()
                                            {
                                                {
                                                    projectOp.Children[0].Id, new SymbolChildUi
                                                                              {
                                                                                  SymbolChild = projectSymbol.Children[0],
                                                                                  Name = "Add1",
                                                                                  Position = new Vector2(100, 100)
                                                                              }
                                                },
                                                {
                                                    projectOp.Children[1].Id, new SymbolChildUi
                                                                              {
                                                                                  SymbolChild = projectSymbol.Children[1],
                                                                                  Position = new Vector2(50, 200)
                                                                              }
                                                },
                                                {
                                                    projectOp.Children[2].Id, new SymbolChildUi
                                                                              {
                                                                                  SymbolChild = projectSymbol.Children[2],
                                                                                  Name = "Random",
                                                                                  Position = new Vector2(250, 200)
                                                                              }
                                                },
                                            });

            // create and register input controls
            InputUiRegistry.Entries.Add(typeof(float), new FloatInputUi());
            InputUiRegistry.Entries.Add(typeof(int), new IntInputUi());
            InputUiRegistry.Entries.Add(typeof(string), new StringInputUi());
            OutputUiRegistry.Entries.Add(typeof(float), new FloatOutputUi());
            OutputUiRegistry.Entries.Add(typeof(int), new IntOutputUi());
            OutputUiRegistry.Entries.Add(typeof(string), new StringOutputUi());

            _initialized = true;
            MainOp = projectOp;

            //_nodes.Add(new Node()
            //{
            //    ID = 0,
            //    Name = "MainTex",
            //    Pos = new Vector2(40, 50),
            //    Value = 0.5f,
            //    Color = TColors.White,
            //    InputsCount = 1,
            //    OutputsCount = 1,
            //}
            //);
            //_nodes.Add(new Node()
            //{
            //    ID = 1,
            //    Name = "MainTex2",
            //    Pos = new Vector2(140, 50),
            //    Value = 0.5f,
            //    Color = TColors.White,
            //    InputsCount = 1,
            //    OutputsCount = 1
            //}
            //);
            //_nodes.Add(new Node()
            //{
            //    ID = 2,
            //    Name = "MainTex3",
            //    Pos = new Vector2(240, 50),
            //    Value = 0.5f,
            //    Color = TColors.White,
            //    InputsCount = 1,
            //    OutputsCount = 1
            //}
            //);

            //_links.Add(new NodeLink(0, 0, 2, 0));
            //_links.Add(new NodeLink(1, 0, 2, 1));
        }

        public Instance MainOp;
        private bool _initialized;
    }
}