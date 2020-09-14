using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpellCheck
{
    public class GcsSource
    {
        public string uri { get; set; }

    }
    public class InputConfig
    {
        public GcsSource gcsSource { get; set; }
        public string mimeType { get; set; }

    }
    public class DetectedLanguages
    {
        public string languageCode { get; set; }
        public double confidence { get; set; }

    }
    public class Property
    {
        public IList<DetectedLanguages> detectedLanguages { get; set; }

    }
    public class NormalizedVertices
    {
        public double x { get; set; }
        public double y { get; set; }

    }
    public class BoundingBox
    {
        public IList<NormalizedVertices> normalizedVertices { get; set; }

    }
    public class Symbols
    {
        public string text { get; set; }
        public double confidence { get; set; }

    }
    public class Words
    {
        public BoundingBox boundingBox { get; set; }
        public IList<Symbols> symbols { get; set; }
        public double confidence { get; set; }

    }
    public class Paragraphs
    {
        public BoundingBox boundingBox { get; set; }
        public IList<Words> words { get; set; }
        public double confidence { get; set; }

    }
    public class Blocks
    {
        public BoundingBox boundingBox { get; set; }
        public IList<Paragraphs> paragraphs { get; set; }
        public string blockType { get; set; }
        public double confidence { get; set; }

    }
    public class Pages
    {
        public Property property { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public IList<Blocks> blocks { get; set; }

    }
    public class FullTextAnnotation
    {
        public IList<Pages> pages { get; set; }
        public string text { get; set; }

    }
    public class Context
    {
        public string uri { get; set; }
        public int pageNumber { get; set; }

    }
    public class Responses
    {
        public FullTextAnnotation fullTextAnnotation { get; set; }
        public Context context { get; set; }

    }
    public class RootObject
    {
        public InputConfig inputConfig { get; set; }
        public IList<Responses> responses { get; set; }

    }

}

