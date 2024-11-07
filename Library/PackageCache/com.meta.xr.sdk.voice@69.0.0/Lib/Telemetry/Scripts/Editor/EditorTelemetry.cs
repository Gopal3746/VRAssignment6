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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Meta.Voice.TelemetryUtilities
{
    /// <summary>
    /// This class provides facilities to share telemetry from Unity Editor with Meta.
    /// </summary>
    [InitializeOnLoad]
    internal class EditorTelemetry
    {
        /// <summary>
        /// The editor prefs key holding the logging level.
        /// </summary>
        private const string TELEMETRY_LOGGING_LEVEL = "VoiceSdk.Telemetry.LogLevel";

        /// <summary>
        /// The editor prefs key holding a unique identifier that is set once when consent is obtained and never changes.
        /// This is an approximation of a device ID but is tied to the Unity editor prefs instead, so can change.
        /// </summary>
        private const string TELEMETRY_ENV_ID = "VoiceSdk.Telemetry.EnvironmentId";

        /// <summary>
        /// The active telemetry channel. Used to switch between live and local.
        /// </summary>
        private static TelemetryChannel _instance;

        /// <summary>
        /// Holds the current status of use consent to share telemetry.
        /// </summary>
        private static bool _consentProvided = false;

        /// <summary>
        /// Sequential counter to use as unique instance keys.
        /// </summary>
        private static int _nextEventSequenceId = 1;

        /// <summary>
        /// The ID of the entire session event.
        /// </summary>
        private static int _sessionEventInstanceId = -1;

        /// <summary>
        /// Set to true when telemetry has been initialized.
        /// </summary>
        private static bool _isInitialized;

        /// <summary>
        /// The live telemetry channel. We are caching it to avoid recreating channels when consent changes.
        /// </summary>
        private readonly TelemetryChannel _liveChannel;

        /// <summary>
        /// An ID that is generated once and stored in the preferences. It's similar to a device ID, except it will change
        /// any time it's regenerated or Unity editor preferences are removed.
        /// </summary>
        private static Guid _envId;

        /// <summary>
        /// A random ID given to the session to use for correlation.
        /// </summary>
        private static Guid _sessionID = Guid.NewGuid();

        /// <summary>
        /// Holds annotations for the session that have been sent before the session event was started.
        /// Used only during initialization.
        /// </summary>
        private static Dictionary<AnnotationKey, string> _pendingSessionAnnotations =
            new Dictionary<AnnotationKey, string>();

        internal EditorTelemetry()
        {
            _liveChannel = new TelemetryChannel(this);
            _instance = new LocalTelemetry(this);
#if UNITY_EDITOR_WIN
            EditorApplication.quitting += OnUnityEditorQuitting;
            EditorApplication.update += FirstRunInitialize;
#endif
        }

        private void FirstRunInitialize()
        {
            EditorApplication.update -= FirstRunInitialize;
            ExtractEnvironmentId();
            ConsentProvided = TelemetryConsentManager.ConsentProvided;
            Initialize();
        }

        private void OnUnityEditorQuitting()
        {
            EndEvent(_sessionEventInstanceId, ResultType.Success);
            _isInitialized = false;
            _instance.ShutdownTelemetry();
        }

        internal TelemetryLogLevel LogLevel
        {
            get
            {
                if (_logLevel != TelemetryLogLevel.Unassigned)
                {
                    return _logLevel;
                }

                if (!EditorPrefs.HasKey(TELEMETRY_LOGGING_LEVEL))
                {
                    _logLevel = TelemetryLogLevel.Off;
                }
                else
                {
                    var telemetryLevelString = EditorPrefs.GetString(EditorTelemetry.TELEMETRY_LOGGING_LEVEL);
                    Enum.TryParse(telemetryLevelString, true,
                        out TelemetryLogLevel telemetryLogLevel);
                    _logLevel = telemetryLogLevel;
                }

                return _logLevel;
            }
            set
            {
                EditorPrefs.SetString(EditorTelemetry.TELEMETRY_LOGGING_LEVEL, value.ToString());
                _logLevel = value;
            }
        }

        private TelemetryLogLevel _logLevel = TelemetryLogLevel.Unassigned;

        /// <summary>
        /// Sets consent as obtained or withdrawn. This controls whether Meta will collect telemetry or not
        /// </summary>
        internal bool ConsentProvided
        {
            set
            {
                if (_consentProvided == value)
                {
                    return;
                }

                _consentProvided = value;

                Initialize();

            }
        }

        private void Initialize()
        {
            if (_consentProvided)
            {
                _instance = _liveChannel;
            }
            else
            {
                _instance = new LocalTelemetry(this);
                _isInitialized = false;
                return;
            }

            if (_isInitialized)
            {
                return;
            }

            _sessionEventInstanceId = StartEvent(TelemetryEventId.Session);
            _isInitialized = true;

            if (_pendingSessionAnnotations.Count > 0)
            {
                foreach (var pendingSessionAnnotation in _pendingSessionAnnotations)
                {
                    AnnotateCurrentSession(pendingSessionAnnotation.Key, pendingSessionAnnotation.Value);
                }
            }
        }

        /// <summary>
        /// Logs an event as started.
        /// </summary>
        /// <param name="eventId">The ID of the event that just started.</param>
        /// <returns>Instance key identifying this specific occurrence of the event.</returns>
        internal int StartEvent(TelemetryEventId eventId)
        {
            return _instance.StartEvent(eventId);
        }

        /// <summary>
        /// Logs an instantaneous event that happens at one point in time (as opposed to one with a start and end).
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="annotations">Optional annotations to add to the event.</param>
        /// <returns>Instance key identifying this specific occurrence of the event.</returns>
        internal void LogInstantEvent(TelemetryEventId eventId,
            Dictionary<AnnotationKey, string> annotations = null)
        {
            _instance.LogInstantEvent(eventId, annotations);
        }

        /// <summary>
        /// Annotates an event that has started but did not end yet.
        /// </summary>
        /// <param name="instanceKey">The instance key of the event. Must match the one used starting the event.</param>
        /// <param name="key">The annotation key.</param>
        /// <param name="value">The annotation value.</param>
        internal void AnnotateEvent(int instanceKey, AnnotationKey key,
            string value)
        {
            _instance.AnnotateEvent(instanceKey, key, value);
        }

        /// <summary>
        /// Annotates the current session event.
        /// </summary>
        /// <param name="instanceKey">The instance key of the event. Must match the one used starting the event.</param>
        /// <param name="key">The annotation key.</param>
        /// <param name="value">The annotation value.</param>
        internal void AnnotateCurrentSession(AnnotationKey key, string value)
        {
            if (_isInitialized)
            {
                AnnotateEvent(_sessionEventInstanceId, key, value);
            }
            else
            {
                _pendingSessionAnnotations[key] = value;
            }
        }

        /// <summary>
        /// Logs an event as ended.
        /// </summary>
        /// <param name="instanceKey">The instance key of the event. Must match the one used starting the event.</param>
        /// <param name="result">The result of the event.</param>
        /// <returns>Instance key.</returns>
        internal void EndEvent(int instanceKey, ResultType result)
        {
            _instance.EndEvent(instanceKey, result);
        }

        /// <summary>
        /// Logs an event as ended.
        /// </summary>
        /// <param name="instanceKey">The instance key of the event. Must match the one used starting the event.</param>
        /// <param name="error">The error.</param>
        /// <returns>Instance key.</returns>
        internal void EndEventWithFailure(int instanceKey, string error = null)
        {
            if (!string.IsNullOrEmpty(error))
            {
                _instance.AnnotateEvent(instanceKey, AnnotationKey.Error, error);
            }

            _instance.EndEvent(instanceKey, ResultType.Failure);
        }

        private void ExtractEnvironmentId()
        {
            var keyExists = EditorPrefs.HasKey(TELEMETRY_ENV_ID);
            if (!keyExists)
            {
                _envId = Guid.NewGuid();
                EditorPrefs.SetString(TELEMETRY_ENV_ID, _envId.ToString());
            }
            else
            {
                var idString = EditorPrefs.GetString(TELEMETRY_ENV_ID);
                if (!Guid.TryParse(idString, out _envId))
                {
                    LogWarning($"Failed to parse telemetry environment ID from: {idString}");
                }
            }
        }

        private void LogWarning(object content)
        {
            if (_logLevel == TelemetryLogLevel.Off)
            {
                return;
            }

            Debug.LogWarning(content);
        }

        private void LogVerbose(object content)
        {
            if (_logLevel != TelemetryLogLevel.Verbose)
            {
                return;
            }

            Debug.Log(content);
        }

        private void LogError(object content)
        {
            if (_logLevel == TelemetryLogLevel.Off)
            {
                return;
            }

            Debug.LogError(content);
        }

        /// <summary>
        /// The telemetry channel represents a target for telemetry. The default implementation sends live telemetry
        /// to Meta.
        /// </summary>
        private class TelemetryChannel
        {
            private EditorTelemetry _telemetry;

            internal TelemetryChannel(EditorTelemetry telemetry)
            {
                _telemetry = telemetry;
            }

            private const string PluginName = "SDKTelemetry";

            /// <summary>
            /// Maps the instance keys with their corresponding events.
            /// </summary>
            private Dictionary<int, TelemetryEventId> _instanceKeyMap = new Dictionary<int, TelemetryEventId>();

            /// <summary>
            /// Logs an event as started.
            /// </summary>
            /// <param name="eventId">The ID of the event that just started.</param>
            /// <returns>Instance key identifying this specific occurrence of the event.</returns>
            internal virtual int StartEvent(TelemetryEventId eventId)
            {
                try
                {
                    _instanceKeyMap[_nextEventSequenceId] = eventId;
                    QplMarkerStart((int)eventId, _nextEventSequenceId, -1);
                    AnnotateEvent(_nextEventSequenceId, AnnotationKey.EnvironmentId, _envId.ToString());
                    AnnotateEvent(_nextEventSequenceId, AnnotationKey.SessionId, _sessionID.ToString());
                    AnnotateEvent(_nextEventSequenceId, AnnotationKey.StartTimeStamp,
                        ElapsedMilliseconds.ToString());

                    _telemetry.LogVerbose($"Started telemetry event {eventId}:{_nextEventSequenceId}");
                    return _nextEventSequenceId++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to start event. Exception: {e}");
                    throw;
                }
            }

            /// <summary>
            /// Logs an instantaneous event that happens at one point in time (as opposed to one with a start and end).
            /// </summary>
            /// <param name="eventId">The ID of the event.</param>
            /// <param name="annotations">Optional annotations to add to the event.</param>
            /// <returns>Instance key identifying this specific occurrence of the event.</returns>
            internal virtual void LogInstantEvent(TelemetryEventId eventId,
                Dictionary<AnnotationKey, string> annotations = null)
            {
                var instanceKey = _nextEventSequenceId;
                _instanceKeyMap[instanceKey] = eventId;
                var timeStamp = ElapsedMilliseconds.ToString();
                QplMarkerStart((int)eventId, instanceKey, -1);
                _telemetry.LogVerbose($"Started instant telemetry event {eventId}:{instanceKey}");
                AnnotateEvent(_nextEventSequenceId, AnnotationKey.EnvironmentId, _envId.ToString());
                AnnotateEvent(instanceKey, AnnotationKey.SessionId, _sessionID.ToString());
                AnnotateEvent(instanceKey, AnnotationKey.StartTimeStamp, timeStamp);
                AnnotateEvent(instanceKey, AnnotationKey.EndTimeStamp, timeStamp);

                if (annotations != null)
                {
                    foreach (var annotation in annotations)
                    {
                        AnnotateEvent(instanceKey, annotation.Key, annotation.Value);
                    }
                }

                QplMarkerEnd((int)eventId, ResultType.Success, instanceKey, -1);
                _telemetry.LogVerbose($"Ended instant telemetry event {eventId}:{instanceKey}");
                _instanceKeyMap.Remove(instanceKey);
                ++_nextEventSequenceId;
            }

            /// <summary>
            /// Annotates an event that has started but did not end yet.
            /// </summary>
            /// <param name="eventId">The ID of the event. Should already have started but not ended already.</param>
            /// <param name="instanceKey">The instance key of the event. Must match the one used starting the event.</param>
            /// <param name="key">The annotation key.</param>
            /// <param name="value">The annotation value.</param>
            internal virtual void AnnotateEvent(int instanceKey, AnnotationKey key,
                string value)
            {
                if (!this._instanceKeyMap.ContainsKey(instanceKey))
                {
                    _telemetry.LogWarning($"Attempted to end an event that's invalid or not started. Instance ID: {instanceKey}");
                    return;
                }

                var eventId = _instanceKeyMap[instanceKey];
                QplMarkerAnnotation((int)eventId, key.ToString(), value, instanceKey);
                _telemetry.LogVerbose($"Annotated telemetry event {eventId}:{instanceKey} with {key}:{value}");
            }

            /// <summary>
            /// Logs an event as ended.
            /// </summary>
            /// <param name="eventId">The ID of the event. Should already have started but not ended already.</param>
            /// <param name="instanceKey">The instance key of the event. Must match the one used starting the event.</param>
            /// <param name="result">The result of the event.</param>
            /// <returns>Instance key.</returns>
            internal virtual void EndEvent(int instanceKey, ResultType result)
            {
                if (!this._instanceKeyMap.ContainsKey(instanceKey))
                {
                    _telemetry.LogWarning($"Attempted to end an event that's invalid or not started. Instance ID: {instanceKey}");
                    return;
                }

                var eventId = _instanceKeyMap[instanceKey];
                AnnotateEvent(instanceKey, AnnotationKey.EndTimeStamp,
                    ElapsedMilliseconds.ToString());
                QplMarkerEnd((int)eventId, result, instanceKey, -1);
                _instanceKeyMap.Remove(instanceKey);
                _telemetry.LogVerbose($"Ended telemetry event {eventId}:{instanceKey}({result})");
            }

            public void ShutdownTelemetry()
            {
                try
                {
                    foreach (var instanceKey in _instanceKeyMap.Keys)
                    {
                        AnnotateEvent(instanceKey, AnnotationKey.Error, "Telemetry event not ended gracefully");
                        EndEvent(instanceKey, ResultType.Cancel);
                    }
                }
                finally
                {
                    OnEditorShutdown();
                }
            }

            private static long ElapsedMilliseconds
            {
                get => DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            }

            #region Telemetry native methods
#if UNITY_EDITOR_WIN
            [DllImport(PluginName)]
            private static extern bool QplMarkerStart(int markerId, int instanceKey, long timestampMs);

            [DllImport(PluginName)]
            private static extern bool QplMarkerEnd(int markerId, ResultType boolTypeId,
                int instanceKey, long timestampMs);

            [DllImport(PluginName)]
            private static extern bool QplMarkerPointCached(int markerId, int nameHandle, int instanceKey,
                long timestampMs);

            [DllImport(PluginName)]
            private static extern bool QplMarkerAnnotation(int markerId,
                [MarshalAs(UnmanagedType.LPStr)] string annotationKey,
                [MarshalAs(UnmanagedType.LPStr)] string annotationValue, int instanceKey);

            [DllImport(PluginName)]
            private static extern bool QplCreateMarkerHandle([MarshalAs(UnmanagedType.LPStr)] string name,
                out int nameHandle);

            [DllImport(PluginName)]
            private static extern bool QplDestroyMarkerHandle(int nameHandle);

            [DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool OnEditorShutdown();
#else
            private static bool QplMarkerStart(int markerId, int instanceKey, long timestampMs)
            {
                return false;
            }

            private static bool QplMarkerEnd(int markerId, ResultType boolTypeId,
                int instanceKey, long timestampMs)
            {
                return false;
            }

            private static bool QplMarkerPointCached(int markerId, int nameHandle, int instanceKey,
                long timestampMs)
            {
                return false;
            }

            private static bool QplMarkerAnnotation(int markerId,
                [MarshalAs(UnmanagedType.LPStr)] string annotationKey,
                [MarshalAs(UnmanagedType.LPStr)] string annotationValue, int instanceKey)
            {
                return false;
            }

            private static bool QplCreateMarkerHandle([MarshalAs(UnmanagedType.LPStr)] string name,
                out int nameHandle)
            {
                nameHandle = -1;
                return false;
            }

            private static bool QplDestroyMarkerHandle(int nameHandle)
            {
                return false;
            }

            private static bool OnEditorShutdown()
            {
                return false;
            }
#endif
            #endregion
        }

        /// <summary>
        /// This instance will be used when we don't have consent to collect telemetry.
        /// </summary>
        private class LocalTelemetry : TelemetryChannel
        {
            /// <inheritdoc/>
            internal override int StartEvent(TelemetryEventId eventId)
            {
                return 0;
            }

            /// <inheritdoc/>
            internal override void LogInstantEvent(TelemetryEventId eventId, Dictionary<AnnotationKey, string> annotations = null)
            {
            }

            /// <inheritdoc/>
            internal override void AnnotateEvent(int instanceKey, AnnotationKey key, string value)
            {
            }

            /// <inheritdoc/>
            internal override void EndEvent(int instanceKey, ResultType result)
            {
            }

            internal LocalTelemetry(EditorTelemetry telemetry) : base(telemetry)
            {
            }
        }

        /// <summary>
        /// The result of an event.
        /// </summary>
        public enum ResultType : short
        {
            Success = 2,
            Failure = 3,
            Cancel = 4
        }

        /// <summary>
        /// The event IDs. These should map to GQL IDs.
        /// </summary>
        public enum TelemetryEventId
        {
            Unknown = 92612351,
            Session = 92611421,
            SupplyToken = 92608491,
            CheckAutoTrain = 92612591,
            AutoTrain = 92617773,
            ToggleCheckbox = 1063854409,
            SyncEntities = 92609990,
            ClickButton = 92615194,
            AssignIntentMatcherInInspector = 92616101,
            SelectOption = 92604542,
            GenerateManifest = 92615944,
            LoadManifest = 92613324,
            NavigateToCodeFromInspector = 92614941,
            OpenUi = 92610372,
        }

        /// <summary>
        /// The annotation keys used for the key-value annotations.
        /// </summary>
        public enum AnnotationKey
        {
            Unknown,
            UnrecognizedEvent,
            UnrecognizedAnnotationKey,
            EnvironmentId,
            SessionId,
            StartTimeStamp,
            EndTimeStamp,
            Error,
            PageId,
            CompatibleSignatures,
            IncompatibleSignatures,
            IsAvailable,
            ControlId,
            Value,
            Type,
            WitSdkVersion,
            WitApiVersion
        }
    }
}