#!/usr/bin/env dotnet-script
#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

// ----------------- Entrada -----------------
var inputDir = Args != null && Args.Count > 0
    ? Args[0]
    : Path.Combine(AppContext.BaseDirectory, "input");

if (!Directory.Exists(inputDir))
{
    Console.WriteLine($"Pasta não encontrada: {inputDir}");
    Environment.Exit(1);
}

var files = Directory.GetFiles(inputDir, "*.xml", SearchOption.TopDirectoryOnly);
if (files.Length == 0)
{
    Console.WriteLine("Nenhum XML encontrado na pasta.");
    Environment.Exit(0);
}

foreach (var file in files)
{
    try
    {
        Console.WriteLine($"Processando: {Path.GetFileName(file)}");
        var doc = XDocument.Load(file, LoadOptions.PreserveWhitespace);

        // 0) Remover grupos inteiros
        RemoveGroups(doc, new[] { "infAdic", "infRespTec", "infNFeSupl", "Signature", "protNFe" });

        // 1) Se a raiz for <nfeProc>, remover a tag raiz e manter o conteúdo (principalmente <NFe>)
        UnwrapNfeProc(doc);

        // 2) Anonimizar <emit>, <dest> e <autXML>
        AnonymizeEmit(doc);
        AnonymizeDest(doc);
        AnonymizeAutXml(doc); // <<< novo

        // 3) Ajustar atributo Id de <infNFe>
        FixInfNFeId(doc);

        // 4) Forçar <cDV> = "0" (se existir)
        ForceCDVZero(doc);

        // 5) Salvar com _new.xml
        var newPath = Path.Combine(
            Path.GetDirectoryName(file)!,
            Path.GetFileNameWithoutExtension(file) + "_new.xml"
        );
        doc.Save(newPath);
        Console.WriteLine($"OK -> {Path.GetFileName(newPath)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERRO em {Path.GetFileName(file)}: {ex.Message}");
    }
}

// ----------------- Funções -----------------

// Remove elementos pelo LocalName em qualquer lugar do documento (namespace-agnóstico)
static void RemoveGroups(XDocument doc, IEnumerable<string> localNames)
{
    var names = new HashSet<string>(localNames, StringComparer.OrdinalIgnoreCase);
    var toRemove = doc.Descendants().Where(e => names.Contains(e.Name.LocalName)).ToList();
    foreach (var el in toRemove)
        el.Remove();
}

// Se a raiz for <nfeProc>, substitui a raiz pelo primeiro filho elemento.
// Como já removemos <protNFe>, o esperado é o <NFe>.
static void UnwrapNfeProc(XDocument doc)
{
    var root = doc.Root;
    if (root != null && root.Name.LocalName.Equals("nfeProc", StringComparison.OrdinalIgnoreCase))
    {
        var child = root.Elements().FirstOrDefault();
        if (child != null)
        {
            var newRoot = new XElement(child); // clona pra poder mudar a raiz
            doc.RemoveNodes();
            doc.Add(newRoot);
        }
    }
}

static void AnonymizeEmit(XDocument doc)
{
    var emits = doc.Descendants().Where(e => e.Name.LocalName == "emit");
    foreach (var emit in emits)
    {
        MaskChildText(emit, "CNPJ");
        MaskChildText(emit, "xNome");
        MaskChildText(emit, "xFant");
        MaskChildText(emit, "IE");

        var enderEmit = emit.Elements().FirstOrDefault(e => e.Name.LocalName == "enderEmit");
        if (enderEmit != null)
        {
            MaskChildText(enderEmit, "xLgr");
            MaskChildText(enderEmit, "nro");
            MaskChildText(enderEmit, "xBairro");
            MaskChildText(enderEmit, "CEP");
            MaskChildText(enderEmit, "fone");
        }
    }
}

static void AnonymizeDest(XDocument doc)
{
    var dests = doc.Descendants().Where(e => e.Name.LocalName == "dest");
    foreach (var dest in dests)
    {
        MaskChildText(dest, "CNPJ");
        MaskChildText(dest, "xNome");
        MaskChildText(dest, "IE");

        var enderDest = dest.Elements().FirstOrDefault(e => e.Name.LocalName == "enderDest");
        if (enderDest != null)
        {
            MaskChildText(enderDest, "xLgr");
            MaskChildText(enderDest, "nro");
            MaskChildText(enderDest, "xBairro");
            MaskChildText(enderDest, "CEP");
            MaskChildText(enderDest, "fone");
        }
    }
}

// <<< novo: anonimiza <autXML> tanto CNPJ quanto CPF
static void AnonymizeAutXml(XDocument doc)
{
    var auts = doc.Descendants().Where(e => e.Name.LocalName == "autXML");
    foreach (var aut in auts)
    {
        MaskChildText(aut, "CNPJ");
        MaskChildText(aut, "CPF");
    }
}

static void MaskChildText(XElement parent, string childLocalName)
{
    var el = parent.Elements().FirstOrDefault(e => e.Name.LocalName == childLocalName);
    if (el != null) el.Value = "XXXXXX";
}

static void FixInfNFeId(XDocument doc)
{
    var infNFes = doc.Descendants().Where(e => e.Name.LocalName == "infNFe");
    foreach (var inf in infNFes)
    {
        var idAttr = inf.Attributes().FirstOrDefault(a => a.Name.LocalName == "Id");
        if (idAttr == null) continue;

        var id = idAttr.Value ?? string.Empty;
        int cnpjStartIdx = 8;   // posição 9 (1-based)
        int cnpjLen = 14;

        var chars = id.ToCharArray();

        if (chars.Length >= cnpjStartIdx + cnpjLen)
            for (int i = 0; i < cnpjLen; i++)
                chars[cnpjStartIdx + i] = '9';

        if (chars.Length > 0)
            chars[^1] = '0'; // último caractere = '0'

        idAttr.Value = new string(chars);
    }
}

static void ForceCDVZero(XDocument doc)
{
    var cDVElements = doc
        .Descendants()
        .Where(e => e.Name.LocalName == "ide")
        .Elements()
        .Where(e => e.Name.LocalName == "cDV");

    foreach (var cDV in cDVElements)
        cDV.Value = "0";
}
