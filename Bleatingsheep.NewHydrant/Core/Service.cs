using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Bleatingsheep.NewHydrant.Attributions;
using NLog;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Core
{
    public abstract class Service
    {
        private readonly Lazy<Logger> _logger;

        public Service()
            => _logger = new Lazy<Logger>(
                () => LogFactory?.GetLogger(Hydrant.GetServiceName(this)) ?? LogManager.CreateNullLogger()
            );

        internal LogFactory LogFactory { private get; set; }

        protected Logger Logger => _logger.Value;

        /// <summary>
        /// 解析文本部分命令。
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="message"></param>
        /// <param name="mustBePlainText"></param>
        /// <exception cref="ArgumentNullException"><c>regex</c> 或 <c>message</c> 为 <c>null</c>，或者某个特性标注的 <see cref="ParameterAttribute.GroupName"/> 为 <c>null</c>。</exception>
        /// <exception cref="ArgumentException"><see cref="ParameterAttribute.GroupName"/> 重复。</exception>
        /// <exception cref="Exception"><see cref="PropertyInfo.SetValue(object, object)"/>
        /// 和 <see cref="Convert.ChangeType(object, Type)"/> 可能抛出异常。</exception>
        /// <returns></returns>
        protected bool RegexCommand(Regex regex, ReceivedMessage message, bool mustBePlainText = true)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            bool isPlain = message.TryGetPlainText(out string text);
            return mustBePlainText && !isPlain ? false : RegexCommand(regex, text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="text"></param>
        /// <exception cref="ArgumentNullException"><c>regex</c> 或 <c>text</c> 为 <c>null</c>，或者某个特性标注的 <see cref="ParameterAttribute.GroupName"/> 为 <c>null</c>。</exception>
        /// <exception cref="ArgumentException"><see cref="ParameterAttribute.GroupName"/> 重复。</exception>
        /// <exception cref="Exception"><see cref="PropertyInfo.SetValue(object, object)"/>
        /// 和 <see cref="Convert.ChangeType(object, Type)"/> 可能抛出异常。</exception>
        /// <returns></returns>
        protected bool RegexCommand(Regex regex, string text)
        {
            if (regex == null)
            {
                throw new ArgumentNullException(nameof(regex));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            .Select(pi => new { pi, attr = pi.GetCustomAttribute<ParameterAttribute>() })
                            .Where(pi => pi.attr != null && pi.pi.CanWrite)
                            .ToDictionary(pi => pi.attr.GroupName, pi => pi.pi);

            var match = regex.Match(text);
            if (match.Success)
            {
                foreach (Group group in match.Groups)
                {
                    if (properties.TryGetValue(group.Name, out var pi))
                    {
                        pi.SetValue(this, Convert.ChangeType(group.Value, pi.PropertyType));
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
