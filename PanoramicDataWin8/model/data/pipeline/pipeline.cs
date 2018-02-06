using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.data.pipeline
{
    public class Hyperparam {
        public string Name;
        public object Value;
    }

    public class Source
    {
        public string name;
    }

    public class Input
    {
        public Guid id;
        public string name;
    }
    public class Output
    {
        public Guid id;
        public string name;
    }

    public class Argument
    {
        public string name;
        public object value;
    }

    public class MethodCall {
        public string name;
        public int calls;
        public List<Argument> arguments;
    }


    public class User
    {
        public Guid id;
    }

    public class Step
    {
        public string type; // "PRIMITIVE", "USER", "PIPELINE"
    }

    public class UserStep : Step
    {
        public UserStep() { type = "USER"; }
    }

    public class PipelineStep : Step
    {
        public PipelineStep() { type = "PIPELINE"; }
        public Guid pipeline_id; // id of pipeline to run at this step
        public List<Guid> inputs;
        public List<Guid> outputs;
    }

    
    public class Pipeline
    {
        public enum Context { Pretraining, Evaluation, Manual };

        public string id;

        public Source source;

        public DateTime created;

        public Context context;

        public string name;

        public List<User> users;

        public List<Input> inputs;

        public List<Output> outputs;

        public string description;

        public List<Step> Steps;

        static public void testc()
        {
            var pipeline = new Pipeline() {
                id = Guid.NewGuid().ToString(),
                context = Context.Evaluation,
                name = "bob's pipeline",
                description = "this  computes nothing",
                created = DateTime.Now,
                users = new List<User>(new User[] {
                    new User() { id = Guid.NewGuid() }
                }),
                inputs = new List<Input>(new Input[]
                {
                     new Input() {
                         id = Guid.NewGuid(),
                         name ="pipeline input"
                     }
                }),
                outputs = new List<Output>(new Output[]
                {
                     new Output() {
                         id = Guid.NewGuid(),
                         name ="pipeline output"
                     }
                }),
                source = new Source() { name = "Brown" },
                Steps = new List<Step>(new Step[] {
                    new PrimitiveStep()
                    {
                        primitive = new Primitive()
                        {
                            name = "Scale Iinputs",
                            id = Guid.NewGuid(),
                            version = "1.0",
                            python_path = "//mnt/c/scaleInputs",
                        },
                        arguments = new PrimitiveArguments()
                        {
                            inputs     = new PrimitiveArgument() { type = "CONTAINER", value = Guid.NewGuid() }, // id of available data
                            outputs    = new PrimitiveArgument() { type = "CONTAINER", value = Guid.NewGuid() },
                            extra_data = new PrimitiveArgument() { type = "CONTAINER", value = Guid.NewGuid() },
                            loss       = new PrimitiveArgument() { type = "PRIMITIVE", value = 0 },// o-based index from steps identifying primitive
                            offset     = new PrimitiveArgument() { type = "DATA",      value = Guid.NewGuid() }
                        },
                        hyperparams = new Dictionary<string,object>() {
                            ["P1"] = 99,
                        },
                        outputs = new PrimitiveOutput()
                        {
                            produce = Guid.NewGuid(),
                            produce_Score = Guid.NewGuid()
                        }
                    },
                    new UserStep() { },
                    new PipelineStep()
                    {
                        pipeline_id = Guid.NewGuid(), // id of pipeline to run
                        inputs = new List<Guid>(),
                        outputs = new List<Guid>()
                    }
                } )
            };

            var serialized = JsonConvert.SerializeObject(pipeline);

            var config = (JObject)JsonConvert.DeserializeObject(serialized);

        }
    }
}
