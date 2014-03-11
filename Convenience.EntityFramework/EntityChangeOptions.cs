using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Convenience.EntityFramework
{
    public class EntityChangeOptions<T>
    {
        private PropertyTreeNode _includePaths = new PropertyTreeNode();
        private PropertyTreeNode _ignorePaths = new PropertyTreeNode();
        private readonly EfEntityWriter _writer;
        private object _entity;

        internal EntityChangeOptions(EfEntityWriter writer, object entity)
        {
            AssertUtils.NotNull(writer, "writer");
            _writer = writer;
            _entity = entity;
        }

        public EntityChangeOptions<T> Include(string path)
        {
            _includePaths.Add(path);
            return this;
        }

        public EntityChangeOptions<T> Include<TProperty>(Expression<Func<T, TProperty>> path)
        {
            string include;
            if (!TryParsePath(path.Body, out include) || include == null)
                throw new ArgumentException("Invalid include path expression", "path");

            return Include(include);
        }

        public EntityChangeOptions<T> Ignore(string path)
        {
            _ignorePaths.Add(path);
            return this;
        }

        public void Apply()
        {
            _writer.ApplyUsingPaths(this);
        }

        internal PropertyTreeNode IncludePaths
        {
            get { return _includePaths; }
        }

        internal PropertyTreeNode IgnorePaths
        {
            get { return _ignorePaths; }
        }

        internal object Entity
        {
            get { return _entity; }
            set { _entity = value; }
        }

        public bool ShouldIncludeNavigation(string property)
        {
            if (_ignorePaths.ContainsLeaf(property))
                return false;

            if (_includePaths.Contains(property))
                return true;

            return false;
        }

        public bool ShouldIncludeNavigation(string property, out EntityChangeOptions<T> subTree)
        {
            if (_ignorePaths.ContainsLeaf(property))
            {
                subTree = null;
                return false;
            }

            PropertyTreeNode subTreePath;
            if (_includePaths.Contains(property, out subTreePath))
            {
                subTree = new EntityChangeOptions<T>(_writer, _entity);
                subTree._includePaths = subTreePath ?? new PropertyTreeNode();
                subTree._ignorePaths = _ignorePaths.SubTree(property) ?? new PropertyTreeNode();
                return true;
            }

            subTree = null;
            return false;
        }

        #region Parsing selector expressions

        // <summary>
        // Parses a property selector expression used for the expression-based versions of the Property, Collection, Reference,
        // etc methods on <see cref="System.Data.Entity.Infrastructure.DbEntityEntry" /> and
        // <see cref="System.Data.Entity.Infrastructure.DbEntityEntry{T}" /> classes.
        // </summary>
        // <typeparam name="TEntity"> The type of the entity. </typeparam>
        // <typeparam name="TProperty"> The type of the property. </typeparam>
        // <param name="property"> The property. </param>
        // <param name="methodName"> Name of the method. </param>
        // <param name="paramName"> Name of the param. </param>
        // <returns> The property name. </returns>
        public static string ParsePropertySelector<TEntity, TProperty>(
            Expression<Func<TEntity, TProperty>> property, string methodName, string paramName)
        {
            AssertUtils.NotNull(property);

            string path;
            if (!TryParsePath(property.Body, out path) || path == null)
                throw new ArgumentException("Invalid property path ('{1}') for entity {0}", paramName);
            return path;
        }

        // <summary>
        // Called recursively to parse an expression tree representing a property path such
        // as can be passed to Include or the Reference/Collection/Property methods of <see cref="InternalEntityEntry" />.
        // This involves parsing simple property accesses like o =&gt; o.Products as well as calls to Select like
        // o =&gt; o.Products.Select(p =&gt; p.OrderLines).
        // </summary>
        // <param name="expression"> The expression to parse. </param>
        // <param name="path"> The expression parsed into an include path, or null if the expression did not match. </param>
        // <returns> True if matching succeeded; false if the expression could not be parsed. </returns>
        public static bool TryParsePath(Expression expression, out string path)
        {
            AssertUtils.NotNull(expression);

            path = null;
            var withoutConvert = RemoveConvert(expression); // Removes boxing
            var memberExpression = withoutConvert as MemberExpression;
            var callExpression = withoutConvert as MethodCallExpression;

            if (memberExpression != null)
            {
                var thisPart = memberExpression.Member.Name;
                string parentPart;
                if (!TryParsePath(memberExpression.Expression, out parentPart))
                {
                    return false;
                }
                path = parentPart == null ? thisPart : (parentPart + "." + thisPart);
            }
            else if (callExpression != null)
            {
                if (callExpression.Method.Name == "Select"
                    && callExpression.Arguments.Count == 2)
                {
                    string parentPart;
                    if (!TryParsePath(callExpression.Arguments[0], out parentPart))
                    {
                        return false;
                    }
                    if (parentPart != null)
                    {
                        var subExpression = callExpression.Arguments[1] as LambdaExpression;
                        if (subExpression != null)
                        {
                            string thisPart;
                            if (!TryParsePath(subExpression.Body, out thisPart))
                            {
                                return false;
                            }
                            if (thisPart != null)
                            {
                                path = parentPart + "." + thisPart;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            return true;
        }

        public static Expression RemoveConvert(Expression expression)
        {
            AssertUtils.NotNull(expression);

            while (expression.NodeType == ExpressionType.Convert
                   || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression;
        }

        #endregion

        internal class PropertyTreeNode
        {
            public PropertyTreeNode()
            {
                Property = string.Empty;
                Children = new List<PropertyTreeNode>();
            }

            public void Add(string path)
            {
                var dotIdx = path.IndexOf('.');
                string property;
                if (dotIdx < 0)
                {
                    property = path;
                    path = null;
                }
                else
                {
                    property = path.Substring(0, dotIdx);
                    path = path.Substring(dotIdx + 1, path.Length - dotIdx - 1);
                }
                var child = Children.FirstOrDefault(c => c.Property == property);
                if (child == null)
                {
                    Children.Add(child = new PropertyTreeNode());
                    child.Property = property;
                }
                if (!string.IsNullOrEmpty(path))
                    child.Add(path);
            }

            public string Property
            { get; set; }

            public List<PropertyTreeNode> Children
            { get; set; }

            public bool HasChildren
            {
                get { return Children != null && Children.Count > 0; }
            }

            public bool Contains(string property)
            {
                if (HasChildren && Children.Any(c => c.Property == property))
                    return true;
                return false;
            }

            public bool Contains(string property, out PropertyTreeNode includeSubTree)
            {
                includeSubTree = null;
                if (!HasChildren)
                    return false;
                includeSubTree = Children.FirstOrDefault(c => c.Property == property);
                if (includeSubTree != null)
                    return true;

                return false;
            }

            public bool ContainsLeaf(string property)
            {
                if (HasChildren && Children.Any(c => c.Property == property && !c.HasChildren))
                    return true;
                return false;
            }

            public PropertyTreeNode SubTree(string startBranch)
            {
                return Children.FirstOrDefault(c => c.Property == startBranch);
            }

            public PropertyTreeNode Clone()
            {
                var clone = new PropertyTreeNode
                {
                    Property = Property,
                    Children = new List<PropertyTreeNode>()
                };
                foreach (var child in Children)
                    clone.Children.Add(child.Clone());
                return clone;
            }
        }
    }
}
