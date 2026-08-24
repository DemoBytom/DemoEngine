// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.Metrics;
using Demo.Engine.Core.ValueObjects;
using Demo.Engine.Observability.Abstractions;

namespace Demo.Engine.Core.Extensions;

public static class CoreInstrumentationExtensions
{
    extension(RenderingSurfaceId renderingSurfaceId)
    {
        /// <summary>
        /// Rendering SurfaceID tag for OpenTelemetry
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<string, object?> ToOTelTag()
            => renderingSurfaceId
                .ToOTelTag("surfaceId");
    }

    extension<T>(T value)
    {
        /// <summary>
        /// Creates OpenTelemetry tag with the given name and value
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public KeyValuePair<string, object?> ToOTelTag(string tagName)
            => new(
                tagName,
                value);
    }

    extension(Units)
    {
        /// <summary>
        /// Frames per second unit symbol
        /// </summary>
        public static string FPS => "fps";

        /// <summary>
        /// Updates per second unit symbol
        /// </summary>
        public static string UPS => "ups";
    }

    extension<TInstrumentation>(TInstrumentation)
        where TInstrumentation : IInstrumentation
    {
        public static Gauge<int> CreateUpsGauge()
            => Instrumentation.Meter.CreateGauge<int>(
                name: "demo.engine.ups.gauge",
                unit: Units.UPS,
                description: "Updates per second");

        public static Histogram<int> CreateUpsHistogram()
            => Instrumentation.Meter.CreateHistogram<int>(
                name: "demo.engine.ups.histogram",
                unit: Units.UPS,
                description: "Updates per second",
                advice: new InstrumentAdvice<int>()
                {
                    // Experimenting with bucket boundaries to see if we can get better distribution of values in the histogram.
                    HistogramBucketBoundaries = [
                        10, 20, 30, 40,
                        50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
                        60, 61, 62, 63, 64, 65, 66, 67, 68, 69,
                        70, 90, 120,
                    ]
                });

        public static Gauge<int> CreateFpsGauge()
            => Instrumentation.Meter.CreateGauge<int>(
                name: "demo.engine.fps.gauge",
                unit: Units.FPS,
                description: "Frames per second");

        public static Histogram<int> CreateFpsHistogram()
            => Instrumentation.Meter.CreateHistogram<int>(
                name: "demo.engine.fps.histogram",
                unit: Units.FPS,
                description: "Frames per second");
    }
}