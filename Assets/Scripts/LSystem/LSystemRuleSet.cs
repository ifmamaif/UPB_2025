using UnityEngine;

[System.Serializable]
public class Rule
{
    [SerializeField]
    private string _key = "F";

    public char key
    {
        get
        {
            if (!string.IsNullOrEmpty(_key) && _key.Length > 0)
                return _key[0];
            else
                return '\0';
        }
        set
        {
            _key = value.ToString();
        }
    }

    public string value;

    public void ValidateKey()
    {
        if (!string.IsNullOrEmpty(_key) && _key.Length > 1)
        {
            _key = _key[0].ToString();
            Debug.LogWarning("Rule key reset to a single character: " + _key);
        }
    }
}