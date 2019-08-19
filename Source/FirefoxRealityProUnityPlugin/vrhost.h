//
// vrhost.h
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0.If a copy of the MPL was not distributed with this
// file, You can obtain one at https ://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019- Mozilla, Inc.
//
// Author(s): Thomas Moore, Philip Lamb
//

// Prototypes for exported functions from vrhost.dll

#pragma once
#include <Windows.h>
#include <stdint.h>

typedef void(*PFN_CREATEVRWINDOW)(UINT* windowId, HANDLE* hTex, HANDLE* hEvt, uint64_t* width, uint64_t* height);
typedef void(*PFN_SENDUIMESSAGE)(UINT nVRWindow, UINT msg, uint64_t wparam, uint64_t lparam);
typedef void(*PFN_CLOSEVRWINDOW)(UINT nVRWindow);

