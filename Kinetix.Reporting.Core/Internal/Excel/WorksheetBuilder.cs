using System.Linq.Expressions;
using System.Reflection;
using ClosedXML.Excel;
using Kinetix.Modeling;
using Kinetix.Modeling.Annotations;
using Kinetix.Reporting.Annotations;
using Kinetix.Reporting.Core.Excel;
using Kinetix.Services;

namespace Kinetix.Reporting.Core.Internal.Excel;

internal class WorksheetBuilder<T> : IWorksheetBuilder<T>
{
    private readonly IList<(string label, Expression<Func<T, object>> selector)> _columns = new List<(string label, Expression<Func<T, object>> selector)>();
    private readonly IExcelBuilder _excelBuilder;
    private readonly IReferenceManager _referenceManager;
    private readonly IXLWorksheet _worksheet;

    private IEnumerable<T> _data;
    private IAsyncEnumerable<T> _dataAsync;
    private int _maxResults;
    private bool _transpose = false;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="referenceManager">ReferenceManager.</param>
    /// <param name="excelBuilder">ExcelBuilder.</param>
    /// <param name="worksheet">Worksheet.</param>
    public WorksheetBuilder(ExcelBuilder excelBuilder, IReferenceManager referenceManager, IXLWorksheet worksheet)
    {
        _excelBuilder = excelBuilder;
        _referenceManager = referenceManager;
        _worksheet = worksheet;
    }

    /// <inheritdoc cref="IWorksheetBuilder{T}.Build" />
    public async Task<IExcelBuilder> Build(Func<IXLWorksheet, Task> postBuildAction)
    {
        for (var i = 0; i < _columns.Count; i++)
        {
            var (label, _) = _columns[i];
            if (_transpose)
            {
                _worksheet.Cell(i + 1, 1).Value = label;
            }
            else
            {
                _worksheet.Cell(1, i + 1).Value = label;
            }
        }

        var itemHandlers = _columns.Select((column, i) =>
        {
            var (_, selector) = column;
            var definition = BeanDescriptor.GetDefinition(typeof(T));
            (string True, string False) booleanFormat = default;
            string dateFormat = null;
            string numberFormat = null;
            Type referenceType = null;

            if (selector.Body is not MemberExpression me)
            {
                me = (selector.Body as UnaryExpression)?.Operand as MemberExpression;
            }

            if (me != null)
            {
                var def = definition;

                if (me.Expression is MethodCallExpression mce)
                {
                    def = BeanDescriptor.GetDefinition(mce.Type);
                }

                var property = def.Properties.SingleOrDefault(p => p.PropertyName == me.Member.Name);
                booleanFormat = property?.Domain.ExtraAttributes.OfType<BooleanFormatAttribute>().SingleOrDefault()?.Format ?? default;
                dateFormat = property?.Domain.ExtraAttributes.OfType<DateFormatAttribute>().SingleOrDefault()?.Format;
                numberFormat = property?.Domain.ExtraAttributes.OfType<NumberFormatAttribute>().SingleOrDefault()?.Format;
                if (property?.ReferenceType?.GetCustomAttributes<ReferenceAttribute>().Any() ?? false)
                {
                    referenceType = property.ReferenceType;
                }
            }

            return (Action<T, int>)((T item, int j) =>
            {
                var cell = _transpose
                    ? _worksheet.Cell(i + 1, j)
                    : _worksheet.Cell(j, i + 1);

                var value = selector.Compile()(item);

                if (referenceType != null)
                {
                    cell.SetValue(_referenceManager.GetReferenceValue(referenceType, value));
                }
                else if (booleanFormat != default && value is bool b)
                {
                    cell.SetValue(b ? booleanFormat.True : booleanFormat.False);
                }
                else
                {
                    cell.SetValue(XLCellValue.FromObject(value));
                }

                if (dateFormat != null)
                {
                    cell.Style.DateFormat.Format = dateFormat;
                }

                if (numberFormat != null)
                {
                    cell.Style.NumberFormat.Format = numberFormat;
                }
            });
        });

        if (_data != null)
        {
            var j = 1;
            foreach (var item in _data)
            {
                j++;
                foreach (var itemHandler in itemHandlers)
                {
                    itemHandler(item, j);
                }

                if (j > _maxResults)
                {
                    break;
                }
            }
        }
        else if (_dataAsync != null)
        {
            var j = 1;
            await foreach (var item in _dataAsync)
            {
                j++;
                foreach (var itemHandler in itemHandlers)
                {
                    itemHandler(item, j);
                }

                if (j > _maxResults)
                {
                    break;
                }
            }
        }

        _worksheet.ColumnsUsed().AdjustToContents();
        if (postBuildAction != null)
        {
            await postBuildAction(_worksheet);

        }
        return _excelBuilder;
    }

    /// <inheritdoc cref="IWorksheetBuilder{T}.Column(Expression{Func{T, object}})" />
    public IWorksheetBuilder<T> Column(Expression<Func<T, object>> selector)
    {
        return Column(string.Empty, selector);
    }

    /// <inheritdoc cref="IWorksheetBuilder{T}.Column(string)" />
    public IWorksheetBuilder<T> Column(string label = null)
    {
        return Column(label, _ => null);
    }

    /// <inheritdoc cref="IWorksheetBuilder{T}.Column(string, Expression{Func{T, object}})" />
    public IWorksheetBuilder<T> Column(string label, Expression<Func<T, object>> selector)
    {
        _columns.Add((label, selector));
        return this;
    }

    /// <inheritdoc cref="IWorksheetBuilder{T}.Data(IEnumerable{T})" />
    public IWorksheetBuilder<T> Data(IEnumerable<T> data)
    {
        _data = data;
        return this;
    }

    /// <inheritdoc cref="IWorksheetBuilder{T}.Data(IAsyncEnumerable{T})" />
    public IWorksheetBuilder<T> Data(IAsyncEnumerable<T> dataAsync)
    {
        _dataAsync = dataAsync;
        return this;
    }

    /// <inheritdoc cref="IWorksheetBuilder{T}.MaxResults" />
    public IWorksheetBuilder<T> MaxResults(int maxResults)
    {
        _maxResults = maxResults;
        return this;
    }

    /// <inheritdoc cref="IWorksheetBuilder{T}.Transpose" />
    public IWorksheetBuilder<T> Transpose()
    {
        _transpose = true;
        return this;
    }
}
