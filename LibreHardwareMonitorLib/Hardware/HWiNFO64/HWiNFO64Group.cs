// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.

using System.Collections.Generic;

namespace LibreHardwareMonitor.Hardware.HWiNFO64;

internal class HWiNFO64Group : IGroup
{
    private readonly List<IHardware> _hardware = new();

    public HWiNFO64Group(ISettings settings)
    {
        _hardware.Add(new HWiNFO64Hardware(settings));
    }

    public IReadOnlyList<IHardware> Hardware => _hardware.AsReadOnly();

    public string GetReport()
    {
        return null;
    }

    public void Close()
    {
        foreach (IHardware hw in _hardware)
            hw.Close();
    }
}
