using System;
using System.ComponentModel;
using Avalonia.Animation;
using Avalonia.Animation.Animators;
using Avalonia.Media.Immutable;

namespace Avalonia.Media
{
    /// <summary>
    /// Describes how an area is painted.
    /// </summary>
    [TypeConverter(typeof(BrushConverter))]
    public abstract class Brush : Animatable, IMutableBrush
    {
        /// <summary>
        /// Defines the <see cref="Opacity"/> property.
        /// </summary>
        public static readonly StyledProperty<double> OpacityProperty =
            AvaloniaProperty.Register<Brush, double>(nameof(Opacity), 1.0);

        /// <summary>
        /// Defines the <see cref="Transform"/> property.
        /// </summary>
        public static readonly StyledProperty<ITransform?> TransformProperty =
            AvaloniaProperty.Register<Brush, ITransform?>(nameof(Transform));

        /// <inheritdoc/>
        public event EventHandler? Invalidated;

        static Brush()
        {
            Animation.Animation.RegisterAnimator<BaseBrushAnimator>(prop => typeof(IBrush).IsAssignableFrom(prop.PropertyType));
            AffectsRender<Brush>(OpacityProperty, TransformProperty);
        }

        /// <summary>
        /// Gets or sets the opacity of the brush.
        /// </summary>
        public double Opacity
        {
            get { return GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }

        /// <summary>
        /// Gets or sets the transform of the brush.
        /// </summary>
        public ITransform? Transform
        {
            get { return GetValue(TransformProperty); }
            set { SetValue(TransformProperty, value); }
        }

        /// <summary>
        /// Parses a brush string.
        /// </summary>
        /// <param name="s">The brush string.</param>
        /// <returns>The <see cref="Color"/>.</returns>
        public static IBrush Parse(string s)
        {
            _ = s ?? throw new ArgumentNullException(nameof(s));

            if (s.Length > 0)
            {
                if (s[0] == '#')
                {
                    return new ImmutableSolidColorBrush(Color.Parse(s));
                }

                var brush = KnownColors.GetKnownBrush(s);
                if (brush != null)
                {
                    return brush;
                }
            }

            throw new FormatException($"Invalid brush string: '{s}'.");
        }

        /// <inheritdoc/>
        public abstract IBrush ToImmutable();

        /// <summary>
        /// Marks a property as affecting the brush's visual representation.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <remarks>
        /// After a call to this method in a brush's static constructor, any change to the
        /// property will cause the <see cref="Invalidated"/> event to be raised on the brush.
        /// </remarks>
        protected static void AffectsRender<T>(params AvaloniaProperty[] properties)
            where T : Brush
        {
            static void Invalidate(AvaloniaPropertyChangedEventArgs e)
            {
                (e.Sender as T)?.RaiseInvalidated(EventArgs.Empty);
            }

            foreach (var property in properties)
            {
                property.Changed.Subscribe(e => Invalidate(e));
            }
        }

        /// <summary>
        /// Raises the <see cref="Invalidated"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected void RaiseInvalidated(EventArgs e) => Invalidated?.Invoke(this, e);
    }
}
