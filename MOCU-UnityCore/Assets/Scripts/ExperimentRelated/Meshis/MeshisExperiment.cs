using DaemonsRelated;
using RacistExperiment;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;


namespace MeshisExperiment
{
    public class MeshisExperiment
    {
        private List<MeshisTrial> _trials;
        private MeshisParameters _config;
        private int _multiplierIndex;
        private float _multiplier;
        private int _currentTrialIndex;

        public DateTime StartedAt;
        public DateTime FinishedAt;
        public bool HasFinished;

        public MeshisExperiment(MeshisParameters config)
        {
            _config = config;
            _trials = new();
            _multiplierIndex = _config.Multipliers.Count - 1;
            _multiplier = _config.Multipliers[_multiplierIndex];
            _currentTrialIndex = 0;
            HasFinished = false;
        }

        public string GetTrialsData()
        {
            return JsonHelper.SerializeJson(_trials);
        }

        public int GetCurrentTrialIndex()
        {
            return _currentTrialIndex;
        }

        public void GenerateTrials()
        {
            if (_config.TrialsNumber < 4)
                throw new Exception("Number of trials must be at least 4");

            int quarter = (int)Math.Floor(_config.TrialsNumber * 0.25);

            _trials.Clear();

            AddTrials(TwoIntervalDistanceType.Reference, TwoIntervalDirection.Straight, quarter);
            AddTrials(TwoIntervalDistanceType.Reference, TwoIntervalDirection.Aside, quarter);
            AddTrials(TwoIntervalDistanceType.Test, TwoIntervalDirection.Straight, quarter);
            AddTrials(TwoIntervalDistanceType.Test, TwoIntervalDirection.Aside, quarter);

            // Random shuffle of all trials
            Shuffle(_trials);

            // First trial has easiest task. All subsequent ones will be calculated only after receiving the response
            SetTrialAdditionalData();
        }

        public void PrepareNextTrial()
        {
            var currentTrial = _trials[_currentTrialIndex];
            var answerWasCorrect = currentTrial.ReceivedAnswer == currentTrial.CorrectAnswer;

            if (currentTrial.ReceivedAnswer == MeshisAnswer.Late)
                LeaveSameDifficulty();
            else if (answerWasCorrect)
                MakeTaskHarder();
            else
                MakeTaskEasier();

            _currentTrialIndex += 1;
            SetTrialAdditionalData();
        }

        public void SetParticipantAnswer(MeshisAnswer answer)
        {
            _trials[_currentTrialIndex].ReceivedAnswer = answer;
        }

        public MeshisTrial StartTrial()
        {
            var trial = _trials[_currentTrialIndex];
            trial.StartedAt = DateTime.UtcNow;
            //Debug.Log($"_multiplier - {_multiplier}");
            return trial;
        }

        public void FinishTrial()
        {
            _trials[_currentTrialIndex].FinishedAt = DateTime.UtcNow;

            if (_currentTrialIndex >= _trials.Count - 1)
            {
                HasFinished = true;
                return;
            }

            PrepareNextTrial();
        }

        public void Save()
        {
            new MeshisSavedData { Trials = _trials, Parameters = _config }.Save();
        }

        // .....................

        private void AddTrials(TwoIntervalDistanceType type, TwoIntervalDirection direction, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var first = new TwoIntervalMeshisExperimentHalf
                {
                    DistanceType = type,
                    Direction = direction
                };

                var second = new TwoIntervalMeshisExperimentHalf
                {
                    DistanceType = GetComplementaryDistanceType(type),
                    Direction = GetComplementaryDirection(direction)
                };

                _trials.Add(new MeshisTrial
                {
                    FirstInterval = first,
                    SecondInterval = second
                });
            }
        }

        private void MakeTaskHarder()
        {
            if (UnityEngine.Random.value > _config.ChanceToMakeTaskHarder)
                return;

            _multiplierIndex = Mathf.Clamp(_multiplierIndex - 1, 0, _config.Multipliers.Count - 1);
            _multiplier = _config.Multipliers[_multiplierIndex];
        }

        private void MakeTaskEasier()
        {
            if (UnityEngine.Random.value > _config.ChanceToMakeTaskEasier)
                return;

            _multiplierIndex = Mathf.Clamp(_multiplierIndex + 1, 0, _config.Multipliers.Count - 1);
            _multiplier = _config.Multipliers[_multiplierIndex];
        }

        private void LeaveSameDifficulty()
        {
            // nothing
        }


        // ...........................................

        private void Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private TwoIntervalDirection GetComplementaryDirection(TwoIntervalDirection direction)
        {
            return direction == TwoIntervalDirection.Aside ? TwoIntervalDirection.Straight : TwoIntervalDirection.Aside;
        }

        private TwoIntervalDistanceType GetComplementaryDistanceType(TwoIntervalDistanceType type)
        {
            return type == TwoIntervalDistanceType.Reference ? TwoIntervalDistanceType.Test : TwoIntervalDistanceType.Reference;
        }

        private void SetTrialAdditionalData()
        {
            var trialData = _trials[_currentTrialIndex];
            var testDistance = _config.ReferenceDistance * _multiplier;

            // Reference or Test
            if (trialData.FirstInterval.DistanceType == TwoIntervalDistanceType.Reference)
            {
                trialData.FirstInterval.Distance = _config.ReferenceDistance;
                trialData.SecondInterval.Distance = testDistance;
            }
            else
            {
                trialData.FirstInterval.Distance = testDistance;
                trialData.SecondInterval.Distance = _config.ReferenceDistance;
            }

            // Correct answer determination
            if (trialData.FirstInterval.Distance > trialData.SecondInterval.Distance)
            {
                if (trialData.FirstInterval.Direction == TwoIntervalDirection.Straight)
                    trialData.CorrectAnswer = MeshisAnswer.StraightWasLonger;
                else
                    trialData.CorrectAnswer = MeshisAnswer.AsideWasLonger;
            }
            else
            {
                if (trialData.SecondInterval.Direction == TwoIntervalDirection.Straight)
                    trialData.CorrectAnswer = MeshisAnswer.StraightWasLonger;
                else
                    trialData.CorrectAnswer = MeshisAnswer.AsideWasLonger;
            }

            // Left or Right
            if (trialData.FirstInterval.Direction == TwoIntervalDirection.Aside)
                trialData.FirstInterval.Distance *= (UnityEngine.Random.value < 0.5f ? -1 : 1);
            else
                trialData.SecondInterval.Distance *= (UnityEngine.Random.value < 0.5f ? -1 : 1);
        }
    }
}