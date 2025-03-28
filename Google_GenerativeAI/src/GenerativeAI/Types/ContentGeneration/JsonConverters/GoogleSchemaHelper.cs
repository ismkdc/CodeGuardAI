﻿using Json.Schema;
using System.Text.Json.Nodes;
using System.Text.Json;
using Json.More;

namespace GenerativeAI.Types;

public static class GoogleSchemaHelper
{
    /// <summary>
    /// Converts a JSON document that contains valid json schema <see href="https://json-schema.org/specification"/> as e.g. 
    /// generated by <code>Microsoft.Extensions.AI.AIJsonUtilities.CreateJsonSchema</code> or <code>JsonSchema.Net</code>'s
    /// <see cref="JsonSchemaBuilder"/> to a subset that is compatible with Google's APIs.
    /// </summary>
    /// <param name="constructedSchema">Generated, valid json schema.</param>
    /// <returns>Subset of the given json schema in a google-comaptible format.</returns>
    public static Schema ConvertToCompatibleSchemaSubset(JsonDocument constructedSchema)
    {
#if NET6_0_OR_GREATER
        var node = constructedSchema.RootElement.AsNode();
        ConvertNullableProperties(node);


        var x1 = node;
        var x2 = JsonSerializer.Serialize(x1);
        var schema = JsonSerializer.Deserialize(x2,SchemaSourceGenerationContext.Default.Schema);
        return schema;
#else
        var schema = JsonSerializer.Deserialize<Schema>(constructedSchema.RootElement.GetRawText());
        return schema;
#endif
    }

    private static void ConvertNullableProperties(JsonNode? node)
    {
        // If the node is an object, look for a "type" property or nested definitions
        if (node is JsonObject obj)
        {
            // If "type" is an array, remove "null" and collapse if it leaves only one type
            if (obj.TryGetPropertyValue("type", out var typeValue) && typeValue is JsonArray array)
            {
                if (array.Count == 2)
                {
                    var notNullTypes = array.Where(x => x is not null && x.GetValue<string>() != "null").ToList();
                    if (notNullTypes.Count == 1)
                    {
                        obj["type"] = notNullTypes[0]!.GetValue<string>();
                        obj["nullable"] = true;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Google's API for strucutured output requires every property to have one defined type, not multiple options. Path: {obj.GetPath()} Schema: {obj.ToJsonString()}");
                    }
                }
                else if (array.Count > 2) 
                {
                    throw new InvalidOperationException($"Google's API for strucutured output requires every property to have one defined type, not multiple options. Path: {obj.GetPath()} Schema: {obj.ToJsonString()}");
                }
            }

            // Recursively convert any nested schema in "properties"
            if (obj.TryGetPropertyValue("properties", out var propertiesNode) &&
                propertiesNode is JsonObject propertiesObj)
            {
                foreach (var property in propertiesObj)
                {
                    ConvertNullableProperties(property.Value);
                }
            }

            if (obj.TryGetPropertyValue("type", out var newTypeValue) 
                && newTypeValue is JsonNode 
                && newTypeValue.GetValueKind() == JsonValueKind.String 
                && "object".Equals(newTypeValue.GetValue<string>(), StringComparison.OrdinalIgnoreCase)
                && propertiesNode is not JsonObject)
            {
                throw new InvalidOperationException($"Google's API for strucutured output requires every object to have predefined properties. Notably, it does not support dictionaries. Path: {obj.GetPath()} Schema: {obj.ToJsonString()}");
            }

            // Recursively convert any nested schema in "items"
            if (obj.TryGetPropertyValue("items", out var itemsNode))
            {
                ConvertNullableProperties(itemsNode);
            }
        }

        // If the node is an array, traverse each element
        if (node is JsonArray arr)
        {
            foreach (var element in arr)
            {
                ConvertNullableProperties(element);
            }
        }
    }
}
