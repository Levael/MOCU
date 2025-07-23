using System.Collections.Generic;
using UnityEngine;


namespace RacistExperiment
{
    public class RacistScene : MonoBehaviour
    {
        public List<GameObject> WhiteModels;
        public List<GameObject> BlackModels;

        private GameObject _currentModel;

        private float _nextActionTime = 0f;

        private void Awake()
        {
            foreach (var model in WhiteModels)
                model?.SetActive(false);

            foreach (var model in BlackModels)
                model?.SetActive(false);
        }

        private void Update()
        {
            if (Time.time >= _nextActionTime)
            {
                if (_currentModel != null)
                {
                    HideModel();
                    _nextActionTime = Time.time + Random.Range(0.1f, 0.5f);
                }
                else
                {
                    ShowRandomModel();
                    _nextActionTime = Time.time + Random.Range(0.7f, 1f);
                }
            }
        }

        public void ShowWhiteModel()
        {
            ShowRandomFromList(WhiteModels);
        }

        public void ShowBlackModel()
        {
            ShowRandomFromList(BlackModels);
        }

        public void HideModel()
        {
            _currentModel?.SetActive(false);
            _currentModel = null;
        }

        private void ShowRandomFromList(List<GameObject> modelList)
        {
            HideModel();

            if (modelList == null || modelList.Count == 0)
            {
                Debug.LogWarning("Список моделей пуст или не назначен.");
                return;
            }

            int index = Random.Range(0, modelList.Count);
            _currentModel = modelList[index];
            _currentModel?.SetActive(true);
        }

        private void ShowRandomModel()
        {
            var allModels = new List<GameObject>();
            allModels.AddRange(WhiteModels);
            allModels.AddRange(BlackModels);

            if (allModels.Count == 0)
                return;

            int index = Random.Range(0, allModels.Count);
            var selected = allModels[index];

            if (selected != null)
            {
                HideModel();
                selected.SetActive(true);
                _currentModel = selected;
            }
        }
    }
}