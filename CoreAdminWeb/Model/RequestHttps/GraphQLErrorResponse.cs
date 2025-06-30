using System.Collections.Generic;

namespace CoreAdminWeb.Model.RequestHttps
{
    public class GraphQLErrorResponse
    {
        public List<GraphQLError> errors { get; set; }
    }

    public class GraphQLError
    {
        public string message { get; set; }
        public GraphQLErrorExtensions extensions { get; set; }
    }

    public class GraphQLErrorExtensions
    {
        public string code { get; set; }
        public string reason { get; set; }
    }
} 