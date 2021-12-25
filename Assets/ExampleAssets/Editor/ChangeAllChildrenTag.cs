using UnityEngine;
using UnityEditor;

public class ChangeAllChildrenTag : MonoBehaviour
{

    [MenuItem("MyTools/Change All Children Tag")]
    static void ChangeTag()
    {
        // �q�G�����L�[��őI�������S�Ă̗v�f
        Transform[] transforms = Selection.transforms;

        foreach (Transform transform in transforms)
        {
            GetChildren(transform);
        }
    }

    static void GetChildren(Transform transform)
    {
        // �V�����ݒ肷��^�O
        string newTag = "Enemy";

        // �^�O��ݒ�
        transform.tag = newTag;

        // �q�v�f���擾
        Transform children = transform.GetComponentInChildren<Transform>();
        if (children.childCount == 0)
        {
            // ������Ȃ���ΏI��
            return;
        }

        foreach (Transform child in children)
        {
            // �q�v�f�̎q�v�f�����l��
            GetChildren(child);
        }
    }
}
