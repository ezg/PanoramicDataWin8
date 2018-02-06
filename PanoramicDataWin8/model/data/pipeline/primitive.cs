using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.data.pipeline
{
    public class Primitive
    {
        public Guid id;
        public string version;
        public string python_path;
        public string name;
    }

    public class PrimitiveArgument
    {
        public string type; // "CONTAINER", "PRIMITIVE", "DATA"
        public object value;
    }

    public class PrimitiveArguments
    {
        public PrimitiveArgument inputs;
        public PrimitiveArgument outputs;
        public PrimitiveArgument extra_data;
        public PrimitiveArgument loss;
        public PrimitiveArgument offset;
    }



    public class PrimitiveOutput
    {
        public Guid produce;
        public Guid produce_Score;
    }

    public class PrimitiveStep : Step
    {
        public PrimitiveStep() { type = "PRIMITIVE";  }
        public Primitive primitive;
        public PrimitiveArguments arguments = new PrimitiveArguments();
        public PrimitiveOutput outputs;
        public Dictionary<string,object> hyperparams;
    }

}
