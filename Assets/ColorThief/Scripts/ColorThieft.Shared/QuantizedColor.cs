using System;
using UnityEngine;

namespace ColorThief
{
    public class QuantizedColor
    {
        public QuantizedColor(Color color, int population)
        {
            Color = color;
            Population = population;
            IsDark = CalculateYiqLuma(color) < 128;
        }

        public Color Color { get; }
        public int Population { get; }
        public bool IsDark { get; }

        public UnityEngine.Color UnityColor
        {
            get
            {
                var color = UnityColor32;
                return color;
            }
        }

        public UnityEngine.Color UnityColor32 => new Color32(Color.R, Color.G, Color.B, Color.A);

        public int CalculateYiqLuma(Color color)
        {
            return Convert.ToInt32(Math.Round((299 * color.R + 587 * color.G + 114 * color.B) / 1000f));
        }
    }
}