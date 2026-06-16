using System.Text;
using System.Text.Json;

namespace AiHighlightsMcpServer.Prompt_Engineering
{
    // https://platform.claude.com/docs/en/build-with-claude/structured-outputs

    public class PromptUtils
    {
        public static string StructuredJsonToString(StructuredPromptRequest request)
        {
            return JsonSerializer.Serialize(request);
        }

        /// <summary>
        /// Converts a structured prompt request in JSON format to a tag-delimited string format as recognised by Claude AI. 
        /// Each field in the 'curatedPrompt' object is converted to a tag-delimited string in the format: 
        /// <fieldName>fieldValue</fieldName>.  The resulting string is a concatenation of all the tag-delimited strings for 
        /// each field in the 'curatedPrompt' object.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string StructuredJsonToTagDelimited(StructuredPromptRequest request)
        {
            // Example of a structured prompt request in JSON format:
            //
            //"curatedPrompt": {
            //      "directive": "Solve the following question.  Which player scored the first goal?",
            //      "context": "This is in the context of a soccer sports match.",          
            //      "outputFormat": "Use a plain text list. Format the output as first name, last name, team, position, period, match time in hours:mins:secs.",
            //      "examples": "'Fred Bloggs; Manchester United; Centre Forward; 1st half, 1:15:30",
            //      "assistantFeedback": "Feed back all the reasoning used to obtain a result"
            //}

            StringBuilder sb = new StringBuilder();

            // extract all the 'curatedPrompt' fields and their values from the request JSON object
            foreach (var field in request.StructuredPrompt.CuratedPrompt.GetType().GetProperties())
            {
                var fieldName = field.Name;
                var fieldValue = field.GetValue(request.StructuredPrompt.CuratedPrompt)?.ToString() ?? string.Empty;

                // create a string in the format: <fieldName>fieldValue</fieldName>
                var xmlField = $"<{fieldName}>{fieldValue}</{fieldName}>";

                // concatenate all the strings together to form a single string
                sb.AppendLine(xmlField);
            }

            return sb.ToString();
        }
    }
}
