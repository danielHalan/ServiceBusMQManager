#region File Information
/********************************************************************
  Project: ServiceBusMQManager.Tests
  File:    JsonParserSpecs.cs
  Created: 2013-02-13

  Author(s):
    Daniel Halan

 (C) Copyright 2013 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using NServiceBus.Profiler.Common.CodeParser;

namespace ServiceBusMQManager.Tests {
  
  [Subject("code parser")]
  public abstract class with_a_json_parser {
    protected static JsonParser Parser;
    protected static IList<CodeLexem> Lexemes;

    Establish context = () => {
      Parser = new JsonParser();
    };
  }

  [Subject("code parser")]
  public class when_parsing_json_array_content : with_a_json_parser {
    protected static string ComplexJson = "[{\"$type\":\"SomeClass, Itq.Com.Domain\",\"Name\":\"Some Name\",\"Description\":\"sdasdsad\",\"TypeId\":101,\"Tags\":[10001],\"dfDfd\":[{\"Id\":1,\"Text\":null,\"ClientId\":\"e8a6bac4-3196-4962-b5a4-1211a6091a85\",\"Type\":2,\"Person\":null,\"Company\":null}],\"FDffdf\":[{\"dfsdfdsf\":null,\"Id\":1,\"Text\":null,\"OpponentId\":\"1bbf6e3d-1681-49d3-a1f4-10cc04339219\",\"Type\":2,\"Person\":null,\"Company\":null}],\"Sdsdsd\":[],\"Managers\":[{\"UserId\":\"56d7da1c-dc29-41a0-9e09-7b4309f5be5d\"}],\"AggregateId\":\"4692b75b-3418-43e3-b1ce-01a5ee3ba315\"}]";

    Because of = () => Lexemes = Parser.Parse(ComplexJson);

    It shoud_parse_all_content = () => Lexemes.Count.ShouldBeGreaterThan(0);
  }

}
