using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{

    private Text _textValue;
    private Image _image;
    private Animator _animator;
    private Vector2 _nextPosition;
    private Vector2 _originPosition;
    private float _lerpValue = 0;
    private float _lerpSpeed = 10f;
    private bool spawned = false;

    public Color BlueColor;
    public Gradient ColorGradient;
    public int Value
    {
        set
        {
            _textValue.text = value.ToString();
            _image.color = value >= 4096 ? BlueColor : ColorGradient.Evaluate(Mathf.Pow(((float)value / 1024 - 1), 5f) + 1);
            if (value > 2)
                AnimateMerge();

        }
    }
    public Vector2 Position
    {
        get { return transform.position; }
        set
        {
            transform.position = _nextPosition;
            _originPosition = transform.position;
            _nextPosition = value;
            _lerpValue = 0;
        }
    }



    // Methods
    private void Awake()
    {
        _textValue = GetComponentInChildren<Text>();
        _image = GetComponent<Image>();
        _animator = GetComponent<Animator>();
        _nextPosition = transform.position;
    }

    private void Update()
    {
        MovePositionTo(_nextPosition);
    }

    private void MovePositionTo(Vector2 value)
    {
        if (spawned)
        {
            _lerpValue += Time.deltaTime * _lerpSpeed;
            float step = -Mathf.Pow(2, -10 * _lerpValue) + 1;
            step = _lerpValue;
            if (step <= 1)
            {
                transform.position = Vector2.Lerp(_originPosition, _nextPosition, step);
            }
            else
            {
                transform.position = _nextPosition;
            }
        }
        else
        {
            transform.position = _originPosition = _nextPosition;
            spawned = true;
        }
    }

    private void AnimateMerge()
    {
        _animator.SetTrigger("Merge");
    }
}
