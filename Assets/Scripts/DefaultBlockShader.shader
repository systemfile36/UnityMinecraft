//����Ƽ�� ���̴� ���� â���� ��Ÿ�� �̸�
Shader "UnityMinecraft/Blocks" {

	Properties{
		//�ؽ��ĸ� �����Ѵ�.
		//name ("display name", 2D) = "defaulttexture" {} �� ���� ���ǵ�
		//�� ������Ƽ�� �̸����� ������(����Ƽ���� _ �� ���۵Ǵ� ��찡 ����
		_MainTex ("Block Texture Atlas", 2D) = "white" {}
	}
	
	SubShader {
		//https://docs.unity3d.com/530/Documentation/Manual/SL-PassTags.html
		//Ű�� ���� ��, ������ ���������ο��� ������ ������ ������ �� ���
		Tags { 
			"Queue"="AlphaTest" 
			"IgnoreProjector"="True" 
			"RenderType"="TransparentCutout" 
		}
		//Level Of Detail�� ����
		LOD 100

		//���� ���� ���� ���̹Ƿ� Off
		Lighting Off
		
		//���̴� ��ü�� �⺻ ���,
		//�׷��� ī���� ���¿� ���̵� ������ ���� ��ħ(����)�� ������
		Pass {
			//���̴� �ڵ� ��
			//����Ƽ�� built-in shader include files�� �⺻ ������
			//Built-in ������ ���������� ȣȯ��
			CGPROGRAM
				//�ʿ��� �Լ� �̸� ����
				#pragma vertex vertFunction
				#pragma fragment fragFunction		
				#pragma _target 2.0

				#include "UnityCG.cginc"
				
				//POSITION ���� ���� �ִ� ������ ��ǥ(�̸� ���ǵ�)
				//TEXCOORD0 ���� ���� �ִ� �ȼ��� �ؽ����� ��ǥ
				//			�ϳ� �̻��� �ؽ��ĸ� �� ���� �����Ƿ� �ڿ� 0�� ����
				//float4�� 4���� ���� ����, Vector3 ���� �������� ���
				//float2�� ���� Vector2 ���� �������� ���
				//fixed4�� �ƹ����� float4�� ���� �Ҽ��� ����...�ΰ�?				


				//�ʿ��� ������ UV �������� �ڵ�����
				//�� appdata ����ü�� ����.
				//color�� ������ ���� �ǹ��Ѵ�.
				struct appdata {
					
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};
				
				//appdata�� ȭ�鿡 �°� ��ȯ�� ���� ��� ��
				struct v2f {
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};
				
				//�ؽ���
				sampler2D _MainTex;
				
				//������ ��
				float GlobalLightLevel;
					
				//appdata�� �޾Ƽ� v2f�� �����Ѵ�.
				v2f vertFunction (appdata v)
				{
					v2f o;
					
					//v2f�� vertex�� appdata�� ���� ���� World ���� ��ǥ��
					//��ũ�� ��ǥ(ī�޶� ���̴�)�� ��ȯ�Ͽ� �ִ´�.
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					o.color = v.color;

					return o;
				}
				
				//fixed4�� ����� ���� 4���� ���� ����
				//��, RGB�� Alpha ���� ���� �÷��� ����
				//v2f�� �޾Ƽ� �ش� ��ġ�� �ؽ��ĸ� ��ȸ �� ��ȯ
				fixed4 fragFunction(v2f i) : SV_Target {
					//tex2D�� TextureLookup�� �����Ѵ�.
					//��, �ش� ��ġ�� �ؽ��ĸ� ��ȸ�Ѵ�.
					fixed4 col = tex2D(_MainTex, i.uv);

					//������ ��⿡ ������ ���� Alpha���� ���� �� 0 �� 1���̷� clamp �Ѵ�.
					//��, ���� �ϳ��ϳ��� �ڽ��� ��⸦ ������.
					float localLightLevel = clamp(GlobalLightLevel + i.color.a, 0, 1);

					//tex2D()�� ���� �޾ƿ� �� ������ ������ �� ��ȯ�Ѵ�.
					
					//clip()�� ���ǿ� ���� �ȼ��� ���ִ� �Լ��̴�.
					//col.a �� Alpha ���� �ǹ��Ѵ�.
					//��, Alpha���� 1 �̸��̸� clip()�� 0�̸��� ���� �Ѿ��.
					//�׷� �ȼ��� ���ش�. 
					//���� ���� ���� ������ ���� �ִ��� ���ٿ��� ������.
					clip(col.a - 1);
					
					//col�� RGBA(0 0 0 1)(������)�� ���� �����ؼ� ��ȯ
					//�� ������ �μ��� weight��, 
					//�� ���� ���� ���� ������ �޶�����
					//��, �� �κ��� �� ��ġ�� �ؽ��� ���� weight�� ���� ������
					//weight�� 1�� ����� ���� �ι�° ���ڿ� �������
					col = lerp(col, float4(0, 0, 0, 1), localLightLevel);
					
					return col;
				}

				ENDCG
		}

	}
}