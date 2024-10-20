export interface SelectTreeModel {
  id: string,
  name: string,
  children?: SelectTreeModel[],
}
export interface ILocationTwinModel extends SelectTreeModel { }

export class SelectTreeErrorModel {
  expParseFailure: boolean = false;
  failedIds: string[] = [];
}

export interface FlatSelectTreeModel extends SelectTreeModel {
  displayName: string;
}

export function FlattenSelectTree(tree: SelectTreeModel[], prefix: string): FlatSelectTreeModel[] {
  return tree.reduce((acc: FlatSelectTreeModel[], node: SelectTreeModel) => {
    const withParentName = prefix.length === 0 ? node.name : `${prefix} > ${node.name}`;
    const flattenedNode: FlatSelectTreeModel = { displayName: withParentName, ...node };
    if (node.children && node.children.length > 0) {
      const children = FlattenSelectTree(node.children, flattenedNode.displayName);
      return [...acc, flattenedNode, ...children];
    } else {
      return [...acc, flattenedNode];
    }
  }, []);
}

export function GetTargetPaths(tree: SelectTreeModel[], target: string[]) {
  if (!tree || tree.length === 0 || !target || target.length === 0)
    return [];
  let result: string[] = [];
  for (const subTree of tree) {
    const queue = [{ node: subTree, path: [subTree.id] }];
    const visited = new Set();
    while (queue.length > 0) {
      const currNode = queue.shift();
      if (!currNode)
        continue;
      const { node, path } = currNode;
      if (target.includes(node.id)) {
        target = target.filter(f => f !== node.id);
        result = [...result, ...path];
        if (target.length === 0)
          return result;
      }

      visited.add(node);
      if (!node.children)
        continue;
      for (const child of node.children) {
        if (!visited.has(child)) {
          queue.push({ node: child, path: [...path, child.id] });
        }
      }
    }
  }
  return result;
}

export const FormatValueAsExpression = (values: SelectTreeModel[]) => {
  if (!values || values.length === 0)
    return '';
  return values.map(m => "[ " + m.id + " ]").join(' | ');
}

const WillowExpressionRegEx = /\[([^\]]+)\]/g; // Match anything inside square brackets

export const UnFormatExpressionIntoValues = (expression: string) => {
  if (!!expression) {
    const matches = expression.match(WillowExpressionRegEx);
    if (matches) {
      // Remove square brackets and trim whitespaces
      return matches.map(match => match.slice(1, -1).trim());
    }
  }
  return [];
}
