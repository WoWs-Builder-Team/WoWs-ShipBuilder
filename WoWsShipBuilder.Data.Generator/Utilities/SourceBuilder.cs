using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WoWsShipBuilder.Data.Generator.Utilities;

internal sealed class SourceBuilder
{
  private const int IndentSize = 4;

  private readonly StringBuilder sb;

  private int currentIndent;

  private LastAction lastAction;

  public SourceBuilder(StringBuilder? sb = null)
  {
    this.sb = sb ?? new StringBuilder();
  }

  private enum LastAction
  {
    None,

    BlockClose,
  }

  public IDisposable Namespace(string ns)
  {
    return this.Block($"namespace {ns}");
  }

  public IDisposable Class(string className)
  {
    return this.Block($"public partial class {className}");
  }

  public IDisposable Record(string recordName)
  {
      return this.Block($"public partial record {recordName}");
  }

  public IDisposable Block(string? line = null)
  {
    this.BlockOpen(line);
    return this.BlockCloseAction();
  }

  public IDisposable PragmaWarning(string warning)
  {
    this.AddBlankLineIfNeeded();
    this.Append($"#pragma warning disable {warning}").AppendNewLine();
    return new DisposableAction(() => this.Append($"#pragma warning restore {warning}").AppendNewLine());
  }

  public SourceBuilder DelimitedLines(string delimiter, params string[] lines) => this.DelimitedLines(delimiter, lines as IReadOnlyList<string>);

  public SourceBuilder DelimitedLines(string delimiter, IEnumerable<string> lines) => this.DelimitedLines(delimiter, lines.ToList());

  public SourceBuilder DelimitedLines(string delimiter, IReadOnlyList<string> lines)
  {
    _ = lines ?? throw new ArgumentNullException(nameof(lines));

    this.AddBlankLineIfNeeded();

    for (var i = 0; i < lines.Count; ++i)
    {
      this.AppendIndented(lines[i]);

      if (i < lines.Count - 1)
      {
        this.Append(delimiter);
      }

      this.AppendNewLine();
    }

    return this;
  }

  public SourceBuilder Line()
  {
    this.AddBlankLineIfNeeded();
    return this.AppendNewLine();
  }

  public SourceBuilder Line(string line)
  {
    this.AddBlankLineIfNeeded();
    return this.AppendIndentedLine(line);
  }

  public SourceBuilder SpacedLine(string line)
  {
    this.AddBlankLineIfNeeded();
    this.AppendIndentedLine(line);
    this.lastAction = LastAction.BlockClose;
    return this;
  }

  public SourceBuilder Lines(params string[] lines)
  {
    this.AddBlankLineIfNeeded();
    return this.DelimitedLines("", lines);
  }

  public SourceBuilder Lines(IEnumerable<string> lines)
  {
    this.AddBlankLineIfNeeded();
    return this.DelimitedLines("", lines.ToList());
  }

  public IDisposable Parens(string line, string? postfix = null)
  {
    this.AddBlankLineIfNeeded();
    this.AppendIndented(line).Append("(").AppendNewLine().IncreaseIndent();
    return new DisposableAction(() => this.DecreaseIndent().AppendIndented(")").Append(postfix).AppendNewLine());
  }

  public override string ToString() => this.sb.ToString();

  private void BlockClose()
  {
    this.DecreaseIndent().AppendIndentedLine("}");
    this.lastAction = LastAction.BlockClose;
  }

  private void BlockOpen(string? line = null)
  {
    this.AddBlankLineIfNeeded();

    if (line is not null)
    {
      this.AppendIndentedLine(line);
    }

    this.AppendIndentedLine("{").IncreaseIndent();
  }

  private SourceBuilder DecreaseIndent()
  {
    --this.currentIndent;
    return this;
  }

  private SourceBuilder IncreaseIndent()
  {
    ++this.currentIndent;
    return this;
  }

  private void AddBlankLineIfNeeded()
  {
    if (this.lastAction != LastAction.BlockClose)
    {
      return;
    }

    this.lastAction = LastAction.None;
    this.AppendNewLine();
  }

  private SourceBuilder Append(string? text)
  {
    if (text is not null)
    {
      this.sb.Append(text);
    }

    return this;
  }

  private SourceBuilder AppendIndent()
  {
    this.sb.Append(' ', this.currentIndent * IndentSize);
    return this;
  }

  private SourceBuilder AppendIndented(string text) => this.AppendIndent().Append(text);

  private SourceBuilder AppendIndentedLine(string text) => this.AppendIndent().Append(text).AppendNewLine();

  private SourceBuilder AppendNewLine()
  {
    this.sb.AppendLine();
    return this;
  }

  private IDisposable BlockCloseAction() => new DisposableAction(this.BlockClose);

  private sealed class DisposableAction : IDisposable
  {
    private readonly Action action;

    public DisposableAction(Action action) => this.action = action;

    public void Dispose() => this.action();
  }
}
