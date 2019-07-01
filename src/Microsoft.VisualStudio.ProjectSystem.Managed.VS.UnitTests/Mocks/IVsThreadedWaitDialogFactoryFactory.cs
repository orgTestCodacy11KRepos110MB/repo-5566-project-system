﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

using Microsoft.VisualStudio.ProjectSystem.VS;
using Microsoft.VisualStudio.Shell.Interop;

using Moq;

using Xunit;

namespace Microsoft.VisualStudio.Mocks
{
    internal static class IVsThreadedWaitDialogFactoryFactory
    {
        private delegate void CreateInstanceCallback(out IVsThreadedWaitDialog2 ppIVsThreadedWaitDialog);

        public static (IVsThreadedWaitDialogFactory dialogFactory, Action cancel) Create(string title = "", string message = "", bool isCancelable = false)
        {
            IVsThreadedWaitDialogCallback callback = null;
            var threadedWaitDialogFactoryMock = new Mock<IVsThreadedWaitDialogFactory>();
            var threadedWaitDialogMock = new Mock<IVsThreadedWaitDialog3>();
            threadedWaitDialogMock.Setup(m => m.StartWaitDialogWithCallback(
                It.IsNotNull<string>(),
                It.IsNotNull<string>(),
                It.Is<string>(s => s == null),
                It.Is<object>(s => s == null),
                It.Is<string>(s => s == null),
                It.IsAny<bool>(),
                It.IsInRange(0, int.MaxValue, Range.Inclusive),
                It.Is<bool>(v => v == false),
                It.Is<int>(i => i == 0),
                It.Is<int>(i => i == 0),
                It.IsNotNull<IVsThreadedWaitDialogCallback>()))
                .Callback((string szWaitCaption,
                           string szWaitMessage,
                           string szProgressText,
                           object varStatusBmpAnim,
                           string szStatusBarText,
                           bool fIsCancelable,
                           int iDelayToShowDialog,
                           bool fShowProgress,
                           int iTotalSteps,
                           int iCurrentStep,
                           IVsThreadedWaitDialogCallback pCallback) =>
                {
                    Assert.Equal(title, szWaitCaption);
                    Assert.Equal(message, szWaitMessage);
                    Assert.Equal(isCancelable, fIsCancelable);
                    callback = pCallback;
                });
            threadedWaitDialogMock.Setup(m => m.EndWaitDialog(out It.Ref<int>.IsAny));
            var threadedWaitDialog = threadedWaitDialogMock.Object;
            threadedWaitDialogFactoryMock
                .Setup(m => m.CreateInstance(out It.Ref<IVsThreadedWaitDialog2>.IsAny))
                .Callback(new CreateInstanceCallback((out IVsThreadedWaitDialog2 ppIVsThreadedWaitDialog) =>
                {
                    ppIVsThreadedWaitDialog = threadedWaitDialog;
                }))
                .Returns(HResult.OK);

            Action cancel = () =>
            {
                callback?.OnCanceled();
            };

            return (threadedWaitDialogFactoryMock.Object, cancel);
        }
    }
}
