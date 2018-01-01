using System;
using System.Collections.Generic;
using UnityEngine;

public class VoxelStruct
{
	public Vector3 Pos;
	public bool Intersected;

	public VoxelStruct(Vector3 pos, bool intersected)
	{
		Pos = pos;
		Intersected = intersected;
	}
}

//[ExecuteInEditMode]
public class Voxel : MonoBehaviour
{
	public enum Axis
	{
		XAxis,
		YAxis,
		ZAxis,
		TOTAL
	}

	public Bounds VoxelBound = new Bounds(Vector3.zero, Vector3.zero);
	public Int32 XDensity = 16;
	public Int32 YDensity = 16;
	public Int32 ZDensity = 16;

	public Vector3 RayOrgin = Vector3.zero;
	public Vector3 RayDirection = Vector3.forward;
	public float RayMax = 100.0f;
	public bool ShowNotInsectedVoxel = false;

	void Start()
	{
		_voxels = new VoxelStruct[XDensity * ZDensity * YDensity];
		float sizeX = VoxelBound.size.x / XDensity;
		float sizeY = VoxelBound.size.y / YDensity;
		float sizeZ = VoxelBound.size.z / ZDensity;
		_voxelSize = new Vector3(sizeX, sizeY, sizeZ);
		for (Int32 iterZ = 0; iterZ < ZDensity; ++iterZ)
		{
			for (Int32 iterY = 0; iterY < YDensity; ++iterY)
			{
				for (Int32 iterX = 0; iterX < XDensity; ++iterX)
				{
					float x = iterX * _voxelSize.x;
					float y = iterY * _voxelSize.y;
					float z = iterZ * _voxelSize.z;
					_voxels[iterX + iterY * XDensity + iterZ * XDensity * YDensity] = new VoxelStruct(new Vector3(x, y, z), false);
				}
			}
		}
	}

	void Update()
	{
		ClearState();
		TreeDDATraversalAlgorithm();
	}

	void ClearState()
	{
		for (Int32 iterZ = 0; iterZ < ZDensity; ++iterZ)
		{
			for (Int32 iterY = 0; iterY < YDensity; ++iterY)
			{
				for (Int32 iterX = 0; iterX < XDensity; ++iterX)
				{
					VoxelStruct iterVoxel = _voxels[iterX + iterY * XDensity + iterZ * XDensity * YDensity];
					iterVoxel.Intersected = false;
				}
			}
		}
	}

	//http://www.cse.yorku.ca/~amana/research/grid.pdf
	void TreeDDATraversalAlgorithm()
	{
		float XMax = 0.0f;
		float YMax = 0.0f;
		float ZMax = 0.0f;
		Int32 XStep = 0;
		Int32 YStep = 0;
		Int32 ZStep = 0;
		float deltaX = 0.0f;
		float deltaY = 0.0f;
		float deltaZ = 0.0f;
		Int32 OutX = 0;
		Int32 OutY = 0;
		Int32 OutZ = 0;
		Int32 xIndex = WorldPosToVoxel(RayOrgin, Axis.XAxis);
		Int32 yIndex = WorldPosToVoxel(RayOrgin, Axis.YAxis);
		Int32 zIndex = WorldPosToVoxel(RayOrgin, Axis.ZAxis);
		if (RayDirection.x >= 0)
		{

			float directX = Mathf.Max(0.000001f, RayDirection.x); //如果为0时说明射线永远到达那个方向的体素，设定为一个很小的数
			XStep = 1;
			deltaX = _voxelSize.x / directX;
			XMax = (VoxelToWorldPos(xIndex + 1, Axis.XAxis) - RayOrgin.x) / directX;
			OutX = XDensity;
		}
		else
		{
			XMax = (VoxelToWorldPos(xIndex, Axis.XAxis) - RayOrgin.x) / RayDirection.x;
			XStep = -1;
			deltaX = -_voxelSize.x / RayDirection.x;
			OutX = -1;
		}
		//
		if (RayDirection.y >= 0)
		{
			float directY = Mathf.Max(0.000001f, RayDirection.y);
			YMax = (VoxelToWorldPos(yIndex + 1, Axis.YAxis) - RayOrgin.y) / directY;
			YStep = 1;
			deltaY = _voxelSize.y / directY;
			OutY = YDensity;
		}
		else
		{
			YMax = (VoxelToWorldPos(yIndex, Axis.YAxis) - RayOrgin.y) / RayDirection.y;
			YStep = -1;
			deltaY = -_voxelSize.y / RayDirection.y;
			OutY = -1;
		}
		//
		if (RayDirection.z >= 0)
		{
			float directZ = Mathf.Max(0.000001f, RayDirection.z);
			ZMax = (VoxelToWorldPos(zIndex + 1, Axis.ZAxis) - RayOrgin.z) / directZ;
			ZStep = 1;
			deltaZ = _voxelSize.z / directZ;
			OutZ = ZDensity;
		}
		else
		{
			ZMax = (VoxelToWorldPos(zIndex, Axis.ZAxis) - RayOrgin.z) / RayDirection.z;
			ZStep = -1;
			deltaZ = -_voxelSize.z / RayDirection.z;
			OutZ = -1;
		}
		//
		for (; ; )
		{
			VoxelStruct voxel = GetVoxelStruct(xIndex, yIndex, zIndex);
			if (voxel != null)
			{
				voxel.Intersected = true;
			}
			//
			if (XMax < YMax)
			{
				if (XMax < ZMax)
				{
					xIndex += XStep;
					if (xIndex == OutX)
					{
						break;
					}
					//
					XMax += deltaX;
				}
				else
				{
					zIndex += ZStep;
					if (zIndex == OutZ)
					{
						break;
					}
					//
					ZMax += deltaZ;
				}
			}
			else
			{
				if (YMax < ZMax)
				{
					yIndex += YStep;
					if (yIndex == OutY)
					{
						break;
					}
					//
					YMax += deltaY;
				}
				else
				{
					zIndex += ZStep;
					if (zIndex == OutZ)
					{
						break;
					}
					//
					ZMax += deltaZ;
				}
			}
		}
	}

	VoxelStruct GetVoxelStruct(Int32 x, Int32 y, Int32 z)
	{
		if (x < 0 || x >= XDensity)
		{
			return null;
		}
		if (y < 0 || y >= YDensity)
		{
			return null;
		}
		if (z < 0 || z >= ZDensity)
		{
			return null;
		}
		//
		return _voxels[x + y * XDensity + z * XDensity * YDensity];
	}

	float VoxelToWorldPos(Int32 p, Axis axis)
	{
		float value = float.MaxValue;
		if (axis == Axis.XAxis)
		{
			value = p * _voxelSize.x + VoxelBound.min.x;
		}
		else if (axis == Axis.YAxis)
		{
			value = p * _voxelSize.y + VoxelBound.min.y;
		}
		else
		{
			value = p * _voxelSize.z + VoxelBound.min.z;
		}
		//
		return value;
	}

	Vector3 VoxelToWorldPos(Int32 x, Int32 y, Int32 z)
	{
		if (x < 0 || x >= XDensity)
		{
			return Vector3.zero;
		}
		if (y < 0 || y >= YDensity)
		{
			return Vector3.zero;
		}
		if (z < 0 || z >= ZDensity)
		{
			return Vector3.zero;
		}
		//
		Vector3 pos = _voxels[x + y * XDensity + z * XDensity * YDensity].Pos;
		pos += VoxelBound.min;
		return pos;
	}

	Int32 WorldPosToVoxel(Vector3 worldPos, Axis axis)
	{
		Int32 value = 0;
		Vector3 voxelPos = worldPos - VoxelBound.min; ;
		if (axis == Axis.XAxis)
		{
			Int32 x = Mathf.FloorToInt(voxelPos.x / _voxelSize.x);
			value = Mathf.Clamp(x, 0, XDensity - 1);
		}
		else if (axis == Axis.YAxis)
		{
			Int32 y = Mathf.FloorToInt(voxelPos.y / _voxelSize.y);
			value = Mathf.Clamp(y, 0, YDensity - 1);
		}
		else
		{
			Int32 z = Mathf.FloorToInt(voxelPos.z / _voxelSize.z);
			value = Mathf.Clamp(z, 0, ZDensity - 1);
		}
		//
		return value;
	}

	Vector3 WorldPosToVoxel(Vector3 worldPos)
	{
		Vector3 voxelPos = worldPos - VoxelBound.min;
		Int32 x = Mathf.FloorToInt(voxelPos.x / _voxelSize.x);
		Int32 y = Mathf.FloorToInt(voxelPos.y / _voxelSize.y);
		Int32 z = Mathf.FloorToInt(voxelPos.z / _voxelSize.z);
		if (x < 0 || x >= XDensity)
		{
			return Vector3.zero;
		}
		if (y < 0 || y >= YDensity)
		{
			return Vector3.zero;
		}
		if (z < 0 || z >= ZDensity)
		{
			return Vector3.zero;
		}
		//
		return _voxels[x + y * XDensity + z * XDensity * YDensity].Pos;
	}


	void OnDrawGizmos()
	{
		if (_voxels == null)
		{
			return;
		}
		//
		Gizmos.color = Color.green;
		Gizmos.DrawLine(RayOrgin, RayOrgin + RayMax * RayDirection);
		float sizeX = VoxelBound.size.x / XDensity;
		float sizeY = VoxelBound.size.y / YDensity;
		float sizeZ = VoxelBound.size.z / ZDensity;
		_voxelSize = new Vector3(sizeX, sizeY, sizeZ);
		Vector3 minCenter = VoxelBound.min + _voxelSize * 0.5f;
		for (Int32 iterZ = 0; iterZ < ZDensity; ++iterZ)
		{
			for (Int32 iterY = 0; iterY < YDensity; ++iterY)
			{
				for (Int32 iterX = 0; iterX < XDensity; ++iterX)
				{
					VoxelStruct iterVoxel = _voxels[iterX + iterY * XDensity + iterZ * XDensity * YDensity];
					if (iterVoxel.Intersected)
					{
						Gizmos.color = Color.green;
						Gizmos.DrawWireCube(iterVoxel.Pos + minCenter, _voxelSize);
					}
					else if (ShowNotInsectedVoxel)
					{
						Gizmos.color = Color.white;
						Gizmos.DrawWireCube(iterVoxel.Pos + minCenter, _voxelSize);
					}
				}
			}
		}
	}

	VoxelStruct[] _voxels;
	Vector3 _voxelSize;
}
