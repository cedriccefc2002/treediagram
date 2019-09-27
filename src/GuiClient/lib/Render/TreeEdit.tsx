import * as until from "util";

import * as React from "react";

import { TreeDiagram as TreeDiagram_Tree } from "../../Ice/Tree";
import { TreeDiagram as TreeDiagram_TreeView } from "../../Ice/TreeView";

import { ClientEvent } from "../IceProxy/ClientEvent";
import { Proxy } from "../IceProxy/IceProxy";
import { Tree } from "../IceProxy/Tree";

import echarts from "echarts";
import ReactEcharts from "echarts-for-react";

import { ITreeEditPropsConfig as R_NodeConfig, Node as R_Node } from "./Node";

import { getLogger } from "log4js";

const logger = getLogger("TreeEdit");

export interface ITreeEditProps {
    uuid: string;
    type: TreeDiagram_Tree.TreeType;
    CallBackHandler: () => void;
}

export interface ITreeListState {
    view: TreeDiagram_TreeView.TreeView | null;
    moveUUID: string;
}

export class TreeEdit extends React.Component<ITreeEditProps, ITreeListState> {
    private proxy: Proxy | null = null;
    private chart: ReactEcharts | null = null;

    public constructor(props: ITreeEditProps) {
        super(props);
        this.state = {
            view: null,
            moveUUID: "",
        };
        this.init();
    }

    public render() {
        const view = this.state.view;
        logger.info(until.inspect(view));
        const nodeConfig: R_NodeConfig = {
            rootUUID: this.props.uuid,
            parentUUID: this.props.uuid,
            uuid: this.props.uuid,
            data: "",
            view,
            isRoot: true,
            isbinaryTree: this.props.type === TreeDiagram_Tree.TreeType.Binary,
            isBinaryleft: false,
        };
        return <>
            <h1 >UUID = {this.props.uuid} {this.props.type.toString()}</h1>
            <h1 >type = {this.props.type.toString()}</h1>
            <button type="button" className="btn btn-success" onClick={this.props.CallBackHandler} >回圖清單</button>
            <hr />
            <h1>編輯：</h1>
            <R_Node config={nodeConfig} moveUUID={this.state.moveUUID} setMoveUUID={(moveUUID) => { this.setState({ moveUUID }); }} />
            <hr />
            <h1>檢視：</h1>
            <ReactEcharts ref={(e) => { this.chart = e; }} onChartReady={() => { this.updateChart(); }} option={this.getChartOption()} />
            <hr />
            <button type="button" className="btn btn-success" onClick={this.props.CallBackHandler} >回圖清單</button>
        </>;
    }
    public componentWillUnmount() {
        if (this.proxy !== null) {
            this.proxy.event.event.removeListener(ClientEvent.eventTreeUpdate, this.eventTreeUpdateListener);
            this.proxy.event.event.removeListener(ClientEvent.eventTreeListUpdate, this.eventTreeListUpdateListener);
        }
    }
    private async updateChart() {
        interface IChild {
            name: string;
            children: IChild[];
        }
        const findChildren = (uuid: string, view: TreeDiagram_TreeView.TreeView) => {
            const children: IChild[] = [];
            for (const rel of view.rels) {
                if (rel.parentUUID === uuid) {
                    const childIndex = view.nodes.findIndex((n) => {
                        return n.uuid === rel.childUUID;
                    });
                    if (childIndex >= 0) {
                        const node = view.nodes[childIndex];
                        children.push({
                            name: node.data,
                            children: findChildren(node.uuid, view),
                        });
                    }
                }
            }
            return children;
        };
        if (this.state.view !== null) {
            if (this.chart !== null) {
                const echart: echarts.ECharts = (this.chart as any).getEchartsInstance();
                echart.setOption({
                    series: [
                        {
                            type: "tree",
                            data: [{
                                name: "root",
                                children: findChildren(this.props.uuid, this.state.view),
                            }],
                            top: 20,
                            left: 50,
                            bottom: 20,
                            right: 150,
                            initialTreeDepth: -1,
                            symbolSize: 20,
                            label: {
                                normal: {
                                    position: "left",
                                    verticalAlign: "middle",
                                    align: "right",
                                    fontSize: 20,
                                },
                            },
                            leaves: {
                                label: {
                                    // normal: {
                                    //     position: "right",
                                    //     verticalAlign: "middle",
                                    //     align: "left",
                                    // },
                                },
                            },
                            expandAndCollapse: true,
                            animationDuration: 550,
                            animationDurationUpdate: 750,
                        },
                    ],
                });
            }
        }
    }
    private getChartOption(): echarts.EChartOption {
        return {
            series: [],
        };
    }

    private eventTreeUpdateListener = async (uuid: string) => {
        if (this.props.uuid === uuid) {
            await this.treeViewUpdate();
        } else {
            logger.info(`${this.props.uuid} === ${uuid}`);
        }
    }

    private eventTreeListUpdateListener = async () => {
        if (this.proxy !== null) {
            const trees = await this.proxy.server_listAllTrees();
            if (trees.findIndex((tree) => tree.uuid === this.props.uuid) === -1) {
                alert("本圖已被刪除");
                this.props.CallBackHandler();
            }
        }
    }

    private async treeViewUpdate() {
        if (this.proxy !== null) {
            const view = await this.proxy.server_getNodeView(this.props.uuid);
            this.setState({ view });
            await this.updateChart();
        }
    }

    private async init() {
        this.proxy = await Proxy.GetProxy();
        this.proxy.event.event.addListener(ClientEvent.eventTreeUpdate, this.eventTreeUpdateListener);
        this.proxy.event.event.addListener(ClientEvent.eventTreeListUpdate, this.eventTreeListUpdateListener);
        return this.treeViewUpdate();
    }
}
