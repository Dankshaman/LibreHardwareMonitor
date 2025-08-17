// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Win32;

namespace LibreHardwareMonitor.Hardware.HWiNFO64;

internal class HWiNFO64Hardware : IHardware
{
    private const string RegistryKeyPath = @"Software\HWiNFO64\VSB";
    private readonly List<ISensor> _sensors = new();
    private readonly ISettings _settings;

    public HWiNFO64Hardware(ISettings settings)
    {
        _settings = settings;
        Update();
    }

    public HardwareType HardwareType => HardwareType.Heatmaster;
    public Identifier Identifier => new("hwinfo64");
    public string Name { get; set; } = "HWiNFO64";
    public IHardware Parent => null;
    public ISensor[] Sensors => _sensors.ToArray();
    public IHardware[] SubHardware => Array.Empty<IHardware>();
    public event SensorEventHandler SensorAdded;
    public event SensorEventHandler SensorRemoved;
    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

    public string GetReport() => null;

    public void Update()
    {
        try
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key == null)
            {
                _sensors.Clear();
                return;
            }

            var values = new Dictionary<string, object>();
            foreach (string valueName in key.GetValueNames())
            {
                values[valueName] = key.GetValue(valueName);
            }

            var newSensors = new List<ISensor>();

            for (int i = 0; i < 1000; i++) // Assume max 1000 sensors
            {
                if (!values.TryGetValue($"Label{i}", out object labelObj))
                    continue;

                string label = labelObj.ToString();

                if (!values.TryGetValue($"ValueRaw{i}", out object valueRawObj))
                    continue;

                if (!float.TryParse(valueRawObj.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
                    continue;

                values.TryGetValue($"Value{i}", out object valueObj);
                string valueString = valueObj?.ToString() ?? string.Empty;

                SensorType sensorType = GetSensorTypeFromUnit(valueString);

                var sensor = _sensors.FirstOrDefault(s => s.Index == i) as HWiNFO64Sensor;
                if (sensor == null)
                {
                    sensor = new HWiNFO64Sensor(label, i, sensorType, this);
                    SensorAdded?.Invoke(sensor);
                }
                else
                {
                    sensor.Name = label;
                }

                sensor.SetValue(value);
                newSensors.Add(sensor);
            }

            var removedSensors = _sensors.Where(s => !newSensors.Any(ns => ns.Identifier == s.Identifier)).ToList();
            foreach (var sensor in removedSensors)
            {
                _sensors.Remove(sensor);
                SensorRemoved?.Invoke(sensor);
            }

            _sensors.Clear();
            _sensors.AddRange(newSensors);

        }
        catch (Exception)
        {
            // Ignore exceptions
        }
    }

    private SensorType GetSensorTypeFromUnit(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return SensorType.Factor;

        if (value.EndsWith("V")) return SensorType.Voltage;
        if (value.EndsWith("A")) return SensorType.Current;
        if (value.EndsWith("W")) return SensorType.Power;
        if (value.EndsWith("MHz")) return SensorType.Clock;
        if (value.Contains("°C")) return SensorType.Temperature;
        if (value.EndsWith("%")) return SensorType.Load;
        if (value.EndsWith("RPM")) return SensorType.Fan;
        if (value.EndsWith("L/h")) return SensorType.Flow;
        if (value.EndsWith("GB")) return SensorType.Data;
        if (value.EndsWith("MB")) return SensorType.SmallData;
        if (value.EndsWith("B/s")) return SensorType.Throughput;

        return SensorType.Factor;
    }

    public void Accept(IVisitor visitor)
    {
        if (visitor == null)
            throw new ArgumentNullException(nameof(visitor));
        visitor.VisitHardware(this);
    }

    public void Close() { }
}
