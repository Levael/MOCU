﻿using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using System;

using DaemonsRelated;
using UnityEngine;


namespace RacistExperiment
{
    public class RacistExperiment
    {
        private List<RacistTrial> _trials;
        private RacistParameters _config;
        private int _multiplierIndex;
        private float _multiplier;
        private int _currentTrialIndex;

        public DateTime StartedAt;
        public DateTime FinishedAt;
        public bool HasFinished;

        public RacistExperiment(RacistParameters config)
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

        public void GenerateTrials()
        {
            if (_config.TrialsNumber < 4)
                throw new Exception("Number of trials must be at least 4");

            int quarter = (int)Math.Floor(_config.TrialsNumber * 0.25);

            _trials.Clear();

            AddTrials(TwoIntervalDistanceType.Reference, TwoIntervalPersonColor.Black, quarter);
            AddTrials(TwoIntervalDistanceType.Reference, TwoIntervalPersonColor.White, quarter);
            AddTrials(TwoIntervalDistanceType.Test, TwoIntervalPersonColor.Black, quarter);
            AddTrials(TwoIntervalDistanceType.Test, TwoIntervalPersonColor.White, quarter);

            // Random shuffle of all trials
            Shuffle(_trials);

            // First trial has easiest task. All subsequent ones will be calculated only after receiving the response
            SetTrialAdditionalData(trialData: _trials[0]);
        }

        public void PrepareNextTrial()
        {
            var currentTrial = _trials[_currentTrialIndex];
            var answerWasCorrect = currentTrial.ReceivedAnswer == currentTrial.CorrectAnswer;

            if (currentTrial.ReceivedAnswer == RacistAnswer.Late)
                LeaveSameDifficulty();
            else if(answerWasCorrect)
                MakeTaskHarder();
            else
                MakeTaskEasier();

            _currentTrialIndex++;
            SetTrialAdditionalData(_trials[_currentTrialIndex]);
        }

        public void SetParticipantAnswer(RacistAnswer answer)
        {
            _trials[_currentTrialIndex].ReceivedAnswer = answer;
        }

        public RacistTrial StartTrial()
        {
            var trial = _trials[_currentTrialIndex];
            trial.StartedAt = DateTime.UtcNow;
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

        // .....................

        private void AddTrials(TwoIntervalDistanceType type, TwoIntervalPersonColor color, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var first = new TwoIntervalExperimentHalf
                {
                    DistanceType = type,
                    PersonColor = color
                };

                var second = new TwoIntervalExperimentHalf
                {
                    DistanceType = GetComplementaryDistanceType(type),
                    PersonColor = GetComplementaryColor(color)
                };

                _trials.Add(new RacistTrial
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

        private TwoIntervalPersonColor GetComplementaryColor(TwoIntervalPersonColor color)
        {
            return color == TwoIntervalPersonColor.Black ? TwoIntervalPersonColor.White : TwoIntervalPersonColor.Black;
        }

        private TwoIntervalDistanceType GetComplementaryDistanceType(TwoIntervalDistanceType type)
        {
            return type == TwoIntervalDistanceType.Reference ? TwoIntervalDistanceType.Test : TwoIntervalDistanceType.Reference;
        }

        private void SetTrialAdditionalData(RacistTrial trialData)
        {
            var testDistance = _config.ReferenceDistance * _multiplier;

            if (trialData.FirstInterval.DistanceType == TwoIntervalDistanceType.Reference)
                trialData.SecondInterval.Distance = testDistance;
            else
                trialData.FirstInterval.Distance = testDistance;

            trialData.CorrectAnswer =
                trialData.FirstInterval.Distance > trialData.SecondInterval.Distance ?
                RacistAnswer.FirstWasLonger :
                RacistAnswer.SecondWasLonger;
        }
    }
}