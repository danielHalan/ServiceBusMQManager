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
  public class when_parsing_more_complex_json_content : with_a_json_parser {
    protected static string ComplexJson = "[{\"$type\":\"Itq.Legal.Domain.Events.Tenant.CaseRequestCreated, Itq.Legal.Domain\",\"Name\":\"Some case 3\",\"Description\":\"sdasdsad\",\"CaseTypeId\":101,\"CaseTags\":[10001],\"Clients\":[{\"Id\":1,\"Text\":null,\"ClientId\":\"e8a6bac4-3196-4962-b5a4-1211a6091a85\",\"Type\":2,\"Person\":null,\"Company\":null}],\"Opponents\":[{\"LegalRepresentation\":null,\"Id\":1,\"Text\":null,\"OpponentId\":\"1bbf6e3d-1681-49d3-a1f4-10cc04339219\",\"Type\":2,\"Person\":null,\"Company\":null}],\"Participants\":[],\"CaseManagers\":[{\"UserId\":\"56d7da1c-dc29-41a0-9e09-7b4309f5be5d\"}],\"Signature\":{\"User\":{\"Id\":\"56d7da1c-dc29-41a0-9e09-7b4309f5be5d\",\"Name\":\"Daniel Halan\",\"Tenant\":\"wistrand\"},\"Date\":\"2013-02-11T16:56:04.1016026+07:00\",\"Sign\":null},\"AggregateId\":\"4692b75b-3418-43e3-b1ce-01a5ee3ba315\"}]";

    Because of = () => Lexemes = Parser.Parse(ComplexJson);

    It shoud_parse_all_content = () => Lexemes.Count.ShouldBeGreaterThan(0);
    //It shoud_parse_all_comments = () => Lexemes.Count(lx => lx.Type == LexemType.Comment).ShouldEqual(6);
  }

}
