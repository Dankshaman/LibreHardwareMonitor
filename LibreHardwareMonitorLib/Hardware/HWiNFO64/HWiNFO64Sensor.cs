// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.

using System;
using System.Collections.Generic;

namespace LibreHardwareMonitor.Hardware.HWiNFO64;

internal class HWiNFO64Sensor : ISensor
{
    public HWiNFO64Sensor(string name, int index, SensorType sensorType, IHardware hardware)
    {
        Name = name;
        Index = index;
        SensorType = sensorType;
        Hardware = hardware;
    }

    public void SetValue(float? value)
    {
        if (value.HasValue)
        {
            Value = value;
            if (!Min.HasValue || value < Min.Value)
                Min = value;
            if (!Max.HasValue || value > Max.Value)
                Max = value;
        }
        else
        {
            Value = null;
        }
    }

    public IControl Control => null;
    public IHardware Hardware { get; }
    public Identifier Identifier => new("hwinfo64", Index.ToString());
    public int Index { get; }
    public bool IsDefaultHidden => false;
    public float? Max { get; private set; }
    public float? Min { get; private set; }
    public string Name { get; set; }
    public IReadOnlyList<IParameter> Parameters => Array.Empty<IParameter>();
    public SensorType SensorType { get; }
    public float? Value { get; private set; }
    public IEnumerable<SensorValue> Values => Array.Empty<SensorValue>();
    public TimeSpan ValuesTimeWindow { get; set; }

    public void ResetMin() => Min = Value;
    public void ResetMax() => Max = Value;
    public void ClearValues() { }

    public void Accept(IVisitor visitor)
    {
        if (visitor == null)
            throw new ArgumentNullException(nameof(visitor));
        visitor.VisitSensor(this);
    }

    public void Traverse(IVisitor visitor) { }
}
