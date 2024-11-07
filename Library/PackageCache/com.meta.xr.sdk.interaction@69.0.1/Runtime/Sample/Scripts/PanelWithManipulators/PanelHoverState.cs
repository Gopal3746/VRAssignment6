/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelHoverState : MonoBehaviour
{
    public List<Grabbable> grabbables = new();
    private bool hovered;
    public bool Hovered => hovered;
    public event Action<bool> WhenStateChanged = delegate { };
    private void Update()
    {
        var prevHovered = hovered;

        hovered = false;
        foreach (var grabbable in grabbables)
        {
            if (grabbable.PointsCount > 0)
            {
                hovered = true;
                break;
            }
        }
        if (prevHovered != hovered)
        {
            WhenStateChanged(hovered);
        }
    }
}
